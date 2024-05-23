namespace Wice;

public class RenderContext
{
    private RenderContext(IComObject<ID2D1DeviceContext> deviceContext, SurfaceCreationOptions? creationOptions = null, RECT? rect = null)
    {
        DeviceContext = deviceContext;
        SurfaceRect = rect;
        SurfaceCreationOptions = creationOptions;
    }

    public IComObject<ID2D1DeviceContext>? DeviceContext { get; private set; }
    public SurfaceCreationOptions? SurfaceCreationOptions { get; }
    public RECT? SurfaceRect { get; }

    public static void WithRenderContext(IComObject<ID2D1DeviceContext> deviceContext, Action<RenderContext> action, SurfaceCreationOptions? creationOptions = null, RECT? rect = null)
    {
        ArgumentNullException.ThrowIfNull(deviceContext);
        ArgumentNullException.ThrowIfNull(action);
        var rc = new RenderContext(deviceContext, creationOptions, rect);
        try
        {
            action(rc);
        }
        finally
        {
            rc.DeviceContext = null;
        }
    }

    [return: NotNullIfNotNull(nameof(color))]
    public virtual IComObject<ID2D1Brush>? CreateSolidColorBrush(D3DCOLORVALUE? color)
    {
        if (DeviceContext == null)
            throw new InvalidOperationException();

        if (!color.HasValue)
            return null;

        var value = color.Value;
        DeviceContext.Object.CreateSolidColorBrush(value, 0, out var brush).ThrowOnError();
        return new ComObject<ID2D1Brush>(brush);
    }

    public virtual IComObject<T> CreateBitmapBrush<T>(ID2D1Bitmap bitmap, D2D1_BITMAP_BRUSH_PROPERTIES? bitmapBrushProperties = null, D2D1_BRUSH_PROPERTIES? brushProperties = null) where T : ID2D1BitmapBrush
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        if (DeviceContext == null)
            throw new InvalidOperationException();

        DeviceContext.Object.CreateBitmapBrush(bitmap, bitmapBrushProperties.CopyToPointer(), brushProperties.CopyToPointer(), out ID2D1BitmapBrush brush).ThrowOnError();
        return new ComObject<T>((T)brush);
    }

    public virtual IComObject<ID2D1Brush> CreateLinearGradientBrush(D2D1_LINEAR_GRADIENT_BRUSH_PROPERTIES properties, params D2D1_GRADIENT_STOP[] stops)
    {
        if (stops == null || stops.Length == 0)
            throw new ArgumentException(null, nameof(stops));

        if (DeviceContext == null)
            throw new InvalidOperationException();

        var value = properties;
        DeviceContext.Object.CreateGradientStopCollection(stops, (uint)stops.Length, D2D1_GAMMA.D2D1_GAMMA_2_2, D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP, out var coll).ThrowOnError();
        try
        {
            DeviceContext.Object.CreateLinearGradientBrush(value, 0, coll, out var brush).ThrowOnError();
            return new ComObject<ID2D1Brush>(brush);
        }
        finally
        {
            coll.FinalRelease();
        }
    }

    public IComObject<ID2D1Brush> CreateRadialGradientBrush(D2D1_RADIAL_GRADIENT_BRUSH_PROPERTIES properties, D2D1_GAMMA gamma = D2D1_GAMMA.D2D1_GAMMA_2_2, D2D1_EXTEND_MODE extendMode = D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP, params D2D1_GRADIENT_STOP[] stops)
    {
        if (stops == null || stops.Length == 0)
            throw new ArgumentException(null, nameof(stops));

        if (DeviceContext == null)
            throw new InvalidOperationException();

        var value = properties;
        DeviceContext.Object.CreateGradientStopCollection(stops, (uint)stops.Length, gamma, extendMode, out var coll).ThrowOnError();
        try
        {
            DeviceContext.Object.CreateRadialGradientBrush(value, 0, coll, out var brush).ThrowOnError();
            return new ComObject<ID2D1Brush>(brush);
        }
        finally
        {
            coll.FinalRelease();
        }
    }
}
