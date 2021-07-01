using System;
using System.Runtime.InteropServices;

namespace DirectN
{
#if NET
    [ComImport, Guid("6329D6CA-3366-490E-9DB3-25312929AC51"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDesktopWindowTarget : IInspectable
    {
        [PreserveSig]
        new HRESULT GetIids(out int iidCount, out IntPtr iids);

        [PreserveSig]
        new HRESULT GetRuntimeClassName([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(HStringMarshaler))] out string className);

        [PreserveSig]
        new HRESULT GetTrustLevel(out TrustLevel trustLevel);
#else
    [ComImport, Guid("6329D6CA-3366-490E-9DB3-25312929AC51"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    public interface IDesktopWindowTarget
    {
#endif
        [PreserveSig]
        HRESULT get_IsTopmost(out bool value);
    }
}
