using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace DirectN
{
    public class Monitor
    {
        private Monitor(IntPtr handle)
        {
            Handle = handle;
            var mi = new MONITORINFOEX();
            mi.cbSize = Marshal.SizeOf(mi);
            if (!WindowsFunctions.GetMonitorInfo(handle, ref mi))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            DeviceName = mi.szDevice.ToString();
            Bounds = mi.rcMonitor;
            WorkingArea = mi.rcWork;
            IsPrimary = (mi.dwFlags & WindowsConstants.MONITORINFOF_PRIMARY) == WindowsConstants.MONITORINFOF_PRIMARY;
        }

        public IntPtr Handle { get; }
        public bool IsPrimary { get; }
        public tagRECT WorkingArea { get; }
        public tagRECT Bounds { get; }
        public string DeviceName { get; }

        public DISPLAY_DEVICE? DISPLAY_DEVICE
        {
            get
            {
                foreach (var dd in DirectN.DISPLAY_DEVICE.All)
                {
                    if (dd.DeviceName.EqualsIgnoreCase(DeviceName))
                        return dd;
                }
                return null;
            }
        }

        public override string ToString() => DeviceName;

        public static Monitor FromWindow(IntPtr hwnd, MFW flags = MFW.MONITOR_DEFAULTTONULL)
        {
            var h = WindowsFunctions.MonitorFromWindow(hwnd, (int)flags);
            if (h == IntPtr.Zero)
                return null;

            return new Monitor(h);
        }

        public static Monitor Primary => All.FirstOrDefault(m => m.IsPrimary);
        public static IEnumerable<Monitor> All
        {
            get
            {
                var all = new List<Monitor>();
                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (m, h, rc, p) =>
                {
                    all.Add(new Monitor(m));
                    return true;
                }, IntPtr.Zero);
                return all;
            }
        }

        private delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lParam);

        [DllImport("user32")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);
    }
}
