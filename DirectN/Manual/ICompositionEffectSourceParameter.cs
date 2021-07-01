using System;
using System.Runtime.InteropServices;

namespace DirectN
{
#if NET
    [ComImport, Guid("2d8f9ddc-4339-4eb9-9216-f9deb75658a2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICompositionEffectSourceParameterWinRT : IInspectable
    {
        // IInspectable
        [PreserveSig]
        new HRESULT GetIids(out int iidCount, out IntPtr iids);

        [PreserveSig]
        new HRESULT GetRuntimeClassName([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(HStringMarshaler))] out string className);

        [PreserveSig]
        new HRESULT GetTrustLevel(out TrustLevel trustLevel);

        // ICompositionEffectSourceParameter
        [PreserveSig]
        HRESULT get_Name([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(HStringMarshaler))] out string name);
    }
#endif
}
