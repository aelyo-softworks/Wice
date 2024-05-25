using System.Runtime.InteropServices.Marshalling;

namespace Wice.Interop;

[GeneratedComInterface, Guid("A1BEA8BA-D726-4663-8129-6B5E7927FFA6")]
public partial interface ICompositionTarget : IInspectable
{
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT get_Root(out nint /* Windows::UI::Composition::IVisual */ value);

    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Error)]
    HRESULT put_Root(nint /* Windows::UI::Composition::IVisual */ value);
}
