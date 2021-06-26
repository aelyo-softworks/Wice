using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace DirectN
{
    public sealed class VerticalBlankTicker : IDisposable
    {
        private volatile bool _stop;
        private volatile bool _throw = true;
        private Thread _thread;
        private long _ticks;
        private int _tickDivider = 1;

        public event EventHandler<VerticalBlankTickerEventArgs> Tick;

        public string DeviceName { get; private set; }
        public bool IsRunning { get; private set; }
        public bool ThrowExceptions { get => _throw; set => _throw = value; }
        public int TickDivider { get => _tickDivider; set => _tickDivider = value.Clamp(1); }

        public void EnsureStarted(string deviceName = null, Action<Thread> configure = null)
        {
            if (!IsRunning || (deviceName != null && !deviceName.EqualsIgnoreCase(DeviceName)))
            {
                Start(deviceName, configure);
            }
        }

        public void Dispose() => Stop();

        public bool? Stop(int? waitTimeout = null)
        {
            IsRunning = false;
            var oldThread = Interlocked.Exchange(ref _thread, null);
            if (oldThread != null && oldThread.IsAlive)
            {
                _stop = true;
                if (waitTimeout.HasValue)
                    return oldThread.Join(waitTimeout.Value);

                return null;
            }
            return true;
        }

        public void Start(string deviceName = null, Action<Thread> configure = null, int? vidPnSourceId = null)
        {
            Stop();
            _stop = false;
            IsRunning = true;
            _thread = new Thread(Wait);
            _thread.IsBackground = true;
            _thread.Name = "_vbt_";
            configure?.Invoke(_thread);

            var si = new StartInfo { DeviceName = deviceName, VidPnSourceId = vidPnSourceId };
            _thread.Start(si);
        }

        private class StartInfo
        {
            public string DeviceName;
            public int? VidPnSourceId;
        }

        public void ResetTicks() => _ticks = 0;

        private void Wait(object state)
        {
            var si = (StartInfo)state;
            var deviceName = si.DeviceName.Nullify() ?? DISPLAY_DEVICE.Primary.DeviceName.Nullify();
            DeviceName = deviceName ?? @"\\.\DISPLAY1";

            var oa = new D3DKMT_OPENADAPTERFROMGDIDISPLAYNAME();
            oa.DeviceName = DeviceName;
            var status = D3DKMTOpenAdapterFromGdiDisplayName(ref oa);
            if (status < 0)
            {
                IsRunning = false;
                throw new Win32Exception(status);
            }

            var we = new D3DKMT_WAITFORVERTICALBLANKEVENT();
            we.hAdapter = oa.hAdapter;

            if (si.VidPnSourceId.HasValue)
            {
                we.VidPnSourceId = si.VidPnSourceId.Value;
            }
            else
            {
                if (oa.VidPnSourceId != 0)
                {
                    we.VidPnSourceId = oa.VidPnSourceId;
                }
                else
                {
                    // for some reason, oa.VidPnSourceId is always set to 0 in our experience
                    // so let's parse
                    const string tok = @"\\.\DISPLAY";
                    var pos = DeviceName.LastIndexOf(tok, StringComparison.OrdinalIgnoreCase);
                    if (pos >= 0 && int.TryParse(DeviceName.Substring(tok.Length + pos), out var i) && i > 0)
                    {
                        we.VidPnSourceId = i - 1;
                    }
                }
            }

            var smallCount = 0;
            do
            {
                status = D3DKMTWaitForVerticalBlankEvent(ref we);
                if (status == 0)
                {
                    if (_tickDivider > 1)
                    {
                        smallCount++;
                        if (smallCount != _tickDivider)
                            continue;

                        smallCount = 0;
                    }

                    var e = new VerticalBlankTickerEventArgs(_ticks++);
                    if (_throw)
                    {
                        Tick?.Invoke(null, e);
                    }
                    else
                    {
                        try
                        {
                            Tick?.Invoke(null, e);
                        }
                        catch
                        {
                            // continue
                        }
                    }

                    if (e.Cancel)
                    {
                        Stop();
                    }
                }
                else
                {
                    IsRunning = false;
                    throw new Win32Exception(status);
                }

                if (_stop)
                    break;
            }
            while (true);
            IsRunning = false;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct D3DKMT_OPENADAPTERFROMHDC
        {
            public IntPtr hDc;
            public int hAdapter;
            public long AdapterLuid;
            public int VidPnSourceId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct D3DKMT_OPENADAPTERFROMGDIDISPLAYNAME
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            public int hAdapter;
            public long AdapterLuid;
            public int VidPnSourceId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct D3DKMT_WAITFORVERTICALBLANKEVENT
        {
            public int hAdapter;
            public int hDevice;
            public int VidPnSourceId;
        }

        [DllImport("gdi32")]
        private static extern int D3DKMTWaitForVerticalBlankEvent(ref D3DKMT_WAITFORVERTICALBLANKEVENT Arg1);

        [DllImport("gdi32")]
        private static extern int D3DKMTOpenAdapterFromGdiDisplayName(ref D3DKMT_OPENADAPTERFROMGDIDISPLAYNAME Arg1);
    }
}
