namespace Wice;

public class RenderContext
{
    private RenderContext(IComObject<ID2D1DeviceContext> deviceContext, SurfaceCreationOptions? creationOptions = null, RECT? rect = null)
    {
        DeviceContext = deviceContext;
        SurfaceRect = rect;
        SurfaceCreationOptions = creationOptions;
    }

    public IComObject<ID2D1DeviceContext> DeviceContext { get; private set; }
    public SurfaceCreationOptions? SurfaceCreationOptions { get; }
    public RECT? SurfaceRect { get; }

    public virtual void WithTransform(D2D_MATRIX_3X2_F transform, Action action)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));

        var existing = DeviceContext.GetTransform();
        DeviceContext.SetTransform(existing * transform);
        try
        {
            action();
        }
        finally
        {
            DeviceContext.SetTransform(existing);
        }
    }

    public static void WithRenderContext(IComObject<ID2D1DeviceContext> deviceContext, Action<RenderContext> action, SurfaceCreationOptions? creationOptions = null, RECT? rect = null)
    {
        ExceptionExtensions.ThrowIfNull(deviceContext, nameof(deviceContext));
        ExceptionExtensions.ThrowIfNull(action, nameof(action));
        var rc = new RenderContext(deviceContext, creationOptions, rect);
        try
        {
            action(rc);
        }
        finally
        {
            rc.DeviceContext = null!;
        }
    }

#if !NETFRAMEWORK
    [return: NotNullIfNotNull(nameof(color))]
#endif
    public virtual IComObject<ID2D1Brush>? CreateSolidColorBrush(D3DCOLORVALUE? color)
    {
        if (DeviceContext == null)
            throw new InvalidOperationException();

        if (!color.HasValue)
            return null;

        var value = color.Value;
        DeviceContext.Object.CreateSolidColorBrush(value, IntPtr.Zero, out var brush).ThrowOnError();
        return new ComObject<ID2D1Brush>(brush);
    }

    public virtual IComObject<T> CreateBitmapBrush<T>(ID2D1Bitmap bitmap, D2D1_BITMAP_BRUSH_PROPERTIES? bitmapBrushProperties = null, D2D1_BRUSH_PROPERTIES? brushProperties = null) where T : ID2D1BitmapBrush
    {
        ExceptionExtensions.ThrowIfNull(bitmap, nameof(bitmap));
        if (DeviceContext == null)
            throw new InvalidOperationException();

#if NETFRAMEWORK
        using var bprops = new ComMemory(bitmapBrushProperties);
        using var props = new ComMemory(brushProperties);
        DeviceContext.Object.CreateBitmapBrush(bitmap, bprops.Pointer, props.Pointer, out ID2D1BitmapBrush brush).ThrowOnError();
        return new ComObject<T>((T)brush);
#else
        DeviceContext.Object.CreateBitmapBrush(bitmap, bitmapBrushProperties.CopyToPointer(), brushProperties.CopyToPointer(), out ID2D1BitmapBrush brush).ThrowOnError();
        return new ComObject<T>((T)brush);
#endif
    }

    public virtual IComObject<ID2D1Brush> CreateLinearGradientBrush(D2D1_LINEAR_GRADIENT_BRUSH_PROPERTIES properties, params D2D1_GRADIENT_STOP[] stops)
    {
        if (stops == null || stops.Length == 0)
            throw new ArgumentException(null, nameof(stops));

        if (DeviceContext == null)
            throw new InvalidOperationException();

        var value = properties;
#if NETFRAMEWORK
        DeviceContext.Object.CreateGradientStopCollection(stops, stops.Length, D2D1_GAMMA.D2D1_GAMMA_2_2, D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP, out var coll).ThrowOnError();
#else
        DeviceContext.Object.CreateGradientStopCollection(stops, (uint)stops.Length, D2D1_GAMMA.D2D1_GAMMA_2_2, D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP, out var coll).ThrowOnError();
#endif
        try
        {
            DeviceContext.Object.CreateLinearGradientBrush(value, IntPtr.Zero, coll, out var brush).ThrowOnError();
            return new ComObject<ID2D1Brush>(brush);
        }
        finally
        {
#if NETFRAMEWORK
            if (coll != null)
            {
                Marshal.ReleaseComObject(coll);
            }
#else
            coll.FinalRelease();
#endif
        }
    }

    public IComObject<ID2D1Brush> CreateRadialGradientBrush(D2D1_RADIAL_GRADIENT_BRUSH_PROPERTIES properties, D2D1_GAMMA gamma = D2D1_GAMMA.D2D1_GAMMA_2_2, D2D1_EXTEND_MODE extendMode = D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP, params D2D1_GRADIENT_STOP[] stops)
    {
        if (stops == null || stops.Length == 0)
            throw new ArgumentException(null, nameof(stops));

        if (DeviceContext == null)
            throw new InvalidOperationException();

        var value = properties;
#if NETFRAMEWORK
        DeviceContext.Object.CreateGradientStopCollection(stops, stops.Length, gamma, extendMode, out var coll).ThrowOnError();
#else
        DeviceContext.Object.CreateGradientStopCollection(stops, (uint)stops.Length, gamma, extendMode, out var coll).ThrowOnError();
#endif
        try
        {
            DeviceContext.Object.CreateRadialGradientBrush(value, IntPtr.Zero, coll, out var brush).ThrowOnError();
            return new ComObject<ID2D1Brush>(brush);
        }
        finally
        {
#if NETFRAMEWORK
            if (coll != null)
            {
                Marshal.ReleaseComObject(coll);
            }
#else
            coll.FinalRelease();
#endif
        }
    }
}
