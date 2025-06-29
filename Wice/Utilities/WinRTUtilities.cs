using Windows.Foundation.Metadata;

namespace Wice.Utilities;

public static class WinRTUtilities
{
    private static readonly ConcurrentDictionary<ushort, bool> _apiContractAvailable = new();

    [DllImport("combase")]
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
    private static extern HRESULT RoGetActivationFactory([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(HStringMarshaler))] string activatableClassId, [MarshalAs(UnmanagedType.LPStruct)] Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object factory);
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments

    public static T GetActivationFactory<T>(string activatableClassId, bool throwOnError = true) => (T)GetActivationFactory(activatableClassId, typeof(T).GUID, throwOnError);
    public static object GetActivationFactory(string activatableClassId, Guid iid, bool throwOnError = true)
    {
        if (activatableClassId == null)
            throw new ArgumentNullException(nameof(activatableClassId));

        RoGetActivationFactory(activatableClassId, iid, out var comp).ThrowOnError(throwOnError);
        return comp;
    }

    public static bool Is19H1OrHigher => IsApiContractAvailable(8);

    public static bool IsApiContractAvailable(ushort version)
    {
        if (_apiContractAvailable.TryGetValue(version, out var available))
            return available;

        available = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", version);
        _apiContractAvailable[version] = available;
        return available;
    }

    public static T WinRTCast<T>(this object obj, bool throwOnError = true) where T : class
    {
        if (obj == null)
            return default;

        if (throwOnError)
            return (T)obj;

        return obj as T;
    }

    public static T WinRTCast<T>(this IntPtr unk, bool throwOnError = true) where T : class
    {
        if (unk == IntPtr.Zero)
            return default;

        if (throwOnError)
            return WinRTCast<T>(Marshal.GetObjectForIUnknown(unk), throwOnError);

        try
        {
            return WinRTCast<T>(Marshal.GetObjectForIUnknown(unk), false);
        }
        catch
        {
            return default;
        }
    }

    public static T ComCast<T>(this object obj, bool throwOnError = true) where T : class
    {
        if (obj == null)
            return default;

        if (throwOnError)
        {
            return (T)obj;
        }

        return obj as T;
    }
}
