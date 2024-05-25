using WinRT;

namespace Wice.Utilities;

public static class ICompositionDrawingSurfaceInteropExtensions
{
    public static IComObject<ID2D1DeviceContext> BeginDraw(this CompositionDrawingSurface surface, RECT? rect = null) =>
        surface.As<ICompositionDrawingSurfaceInterop>().BeginDraw<ID2D1DeviceContext>(rect);

    public static IComObject<T> BeginDraw<T>(this CompositionDrawingSurface surface, RECT? rect = null) where T : ID2D1DeviceContext =>
        surface.As<ICompositionDrawingSurfaceInterop>().BeginDraw<T>(rect);

    public static void EndDraw(this CompositionDrawingSurface surface) =>
        surface.As<ICompositionDrawingSurfaceInterop>().EndDraw();
}
