using System;
using System.Runtime.InteropServices;
using Windows.UI.Composition;

namespace DirectN
{
#if NET
    [ComImport, Guid("A1BEA8BA-D726-4663-8129-6B5E7927FFA6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICompositionTarget : IInspectable
    {
        [PreserveSig]
        new HRESULT GetIids(out int iidCount, out IntPtr iids);

        [PreserveSig]
        new HRESULT GetRuntimeClassName([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(HStringMarshaler))] out string className);

        [PreserveSig]
        new HRESULT GetTrustLevel(out TrustLevel trustLevel);
#else
    [ComImport, Guid("A1BEA8BA-D726-4663-8129-6B5E7927FFA6"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    public interface ICompositionTarget
    {
#endif

        Visual Root { get; set; }
    }
}
