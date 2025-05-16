namespace Wice.Utilities;

// equivalent to System.Diagnostics.Eventing.EventProvider that doesn't exist with .NET Core
public sealed class EventProvider : IDisposable
{
    public static Guid DefaultGuid { get; set; } = new("964d4572-adb9-4f3a-8170-fcbecec27467");
    public static readonly EventProvider Default = new(DefaultGuid);

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
        if (message == null || _handle == 0)
            return; // silent fail

        EventWriteString(_handle, level, keyword, message);
    }

    public void Dispose()
    {
        var handle = Interlocked.Exchange(ref _handle, 0);
        if (handle != 0)
        {
            EventUnregister(handle);
        }
    }

    [DllImport("advapi32")]
    private static extern int EventRegister([MarshalAs(UnmanagedType.LPStruct)] Guid providerId, IntPtr enableCallback, IntPtr callbackContext, out long regHandle);

    [DllImport("advapi32")]
    private static extern int EventUnregister(long registrationHandle);

    [DllImport("advapi32")]
    private static extern int EventWriteString(long regHandle, byte level, long keywords, [MarshalAs(UnmanagedType.LPWStr)] string message);
}
