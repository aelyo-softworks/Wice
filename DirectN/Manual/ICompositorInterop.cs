using System;
using System.Runtime.InteropServices;
using Windows.UI.Composition;

namespace DirectN
{
    [ComImport, Guid("25297D5C-3AD4-4C9C-B5CF-E36A38512330"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICompositorInterop
    {
        [PreserveSig]
        HRESULT CreateCompositionSurfaceForHandle(IntPtr swapChain, out ICompositionSurface result);

        [PreserveSig]
        HRESULT CreateCompositionSurfaceForSwapChain([MarshalAs(UnmanagedType.IUnknown)] object swapChain, out ICompositionSurface result);

#if NET
        [PreserveSig]
        HRESULT CreateGraphicsDevice([MarshalAs(UnmanagedType.IUnknown)] object renderingDevice, [MarshalAs(UnmanagedType.IUnknown)] out object result);
#else
        [PreserveSig]
        HRESULT CreateGraphicsDevice([MarshalAs(UnmanagedType.IUnknown)] object renderingDevice, out CompositionGraphicsDevice result);
#endif
    }
}
