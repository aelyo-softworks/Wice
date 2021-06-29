using System;
using System.Runtime.InteropServices;

namespace DirectN
{
#if NET
    [ComImport, Guid("cb51c0ce-8fe6-4636-b202-861faa07d8f3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IGraphicsEffectWinRT : IInspectable
    {
        // IInspectable
        [PreserveSig]
        new HRESULT GetIids(out int iidCount, out IntPtr iids);

        [PreserveSig]
        new HRESULT GetRuntimeClassName([MarshalAs(UnmanagedType.HString)] out string className);

        [PreserveSig]
        new HRESULT GetTrustLevel(out TrustLevel trustLevel);

        // IGraphicsEffect
        [PreserveSig]
        HRESULT get_Name([MarshalAs(UnmanagedType.HString)] out string name);

        [PreserveSig]
        HRESULT put_Name([MarshalAs(UnmanagedType.HString)] string name);
    }
#endif
}
