using System;
using System.Runtime.InteropServices;

namespace DirectN
{
#if NET
    [ComImport, Guid("2C9DB356-E70D-4642-8298-BC4AA5B4865C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICompositionCapabilitiesInteropFactory : IInspectable
    {
        [PreserveSig]
        new HRESULT GetIids(out int iidCount, out IntPtr iids);

        [PreserveSig]
        new HRESULT GetRuntimeClassName([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(HStringMarshaler))] out string className);

        [PreserveSig]
        new HRESULT GetTrustLevel(out TrustLevel trustLevel);
#else
    [ComImport, Guid("2C9DB356-E70D-4642-8298-BC4AA5B4865C"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    public interface ICompositionCapabilitiesInteropFactory
    {
#endif
        [PreserveSig]
        HRESULT GetForWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.IInspectable)] out object result);
    }
}
