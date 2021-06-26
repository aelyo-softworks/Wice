using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace Wice.Utilities
{
    // equivalent to System.Diagnostics.Eventing.EventProvider that doesn't exist with .NET Core
    public sealed class EventProvider : IDisposable
    {
        private long _handle;

        public EventProvider(Guid providerGuid)
        {
            ProviderId = providerGuid;
            var hr = EventRegister(providerGuid, IntPtr.Zero, IntPtr.Zero, out _handle);
            if (hr != 0)
                throw new Win32Exception(hr);
        }

        public Guid ProviderId { get; }

        public void WriteMessageEvent(string message, byte level = 0, long keyword = 0)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (_handle == 0)
                throw new ObjectDisposedException(nameof(EventProvider));

            EventWriteString(_handle, level, keyword, message);
        }

        public void Dispose() => Interlocked.Exchange(ref _handle, 0);

        [DllImport("advapi32")]
        private static extern int EventRegister([MarshalAs(UnmanagedType.LPStruct)] Guid providerId, IntPtr enableCallback, IntPtr callbackContext, out long regHandle);

        [DllImport("advapi32")]
        private static extern int EventUnregister(long registrationHandle);

        [DllImport("advapi32")]
        private static extern int EventWriteString(long regHandle, byte level, long keywords, [MarshalAs(UnmanagedType.LPWStr)] string message);
    }
}
