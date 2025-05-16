namespace Wice.Utilities;

public static class ICompositionDrawingSurfaceInteropExtensions
{
    public static IComObject<ID2D1DeviceContext> BeginDraw(this CompositionDrawingSurface surface, tagRECT? rect = null) =>
        surface.ComCast<ICompositionDrawingSurfaceInterop>().BeginDraw<ID2D1DeviceContext>(rect);

    public static IComObject<T> BeginDraw<T>(this CompositionDrawingSurface surface, tagRECT? rect = null) where T : ID2D1DeviceContext =>
        surface.ComCast<ICompositionDrawingSurfaceInterop>().BeginDraw<T>(rect);

    public static void EndDraw(this CompositionDrawingSurface surface) =>
        surface.ComCast<ICompositionDrawingSurfaceInterop>().EndDraw();
}
