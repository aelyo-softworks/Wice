using System.Runtime.InteropServices.Marshalling;

namespace Wice.Interop;

// https://learn.microsoft.com/windows/win32/api/inspectable/nn-inspectable-iinspectable
[SupportedOSPlatform("windows8.0")]
[GeneratedComInterface, Guid("af86e2e0-b12d-4c6a-9c5a-d7aa65101e90")]
public partial interface IInspectable
{
    // https://learn.microsoft.com/windows/win32/api/inspectable/nf-inspectable-iinspectable-getiids
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT GetIids(out uint iidCount, out nint iids);

    // https://learn.microsoft.com/windows/win32/api/inspectable/nf-inspectable-iinspectable-getruntimeclassname
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT GetRuntimeClassName(out HSTRING className);

    // https://learn.microsoft.com/windows/win32/api/inspectable/nf-inspectable-iinspectable-gettrustlevel
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT GetTrustLevel(out TrustLevel trustLevel);
}
