namespace Wice;

/// <summary>
/// Provides a lightweight wrapper around a Direct2D device context, offering scoped transform utilities
/// and helpers for creating common brush resources during rendering.
/// </summary>
/// <remarks>
/// Instances created via <see cref="WithRenderContext(IComObject{ID2D1DeviceContext}, Action{RenderContext}, SurfaceCreationOptions?, RECT?)"/>
/// are intended for use only within the provided callback. After the callback returns, the underlying
/// <see cref="DeviceContext"/> is invalidated and must not be used.
/// </remarks>
public class RenderContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenderContext"/> class.
    /// </summary>
    /// <param name="deviceContext">The Direct2D device context to operate on.</param>
    /// <param name="creationOptions">Optional surface creation options that may influence rendering behavior.</param>
    /// <param name="rect">Optional surface rectangle describing the target render area.</param>
    private RenderContext(IComObject<ID2D1DeviceContext> deviceContext, SurfaceCreationOptions? creationOptions = null, RECT? rect = null)
    {
        DeviceContext = deviceContext;
        SurfaceRect = rect;
        SurfaceCreationOptions = creationOptions;
    }

    /// <summary>
    /// Gets the underlying Direct2D device context used for rendering.
    /// </summary>
    /// <remarks>
    /// When created through <see cref="WithRenderContext(IComObject{ID2D1DeviceContext}, Action{RenderContext}, SurfaceCreationOptions?, RECT?)"/>,
    /// this property is invalidated after the callback returns. Do not cache or use it beyond that scope.
    /// </remarks>
    public IComObject<ID2D1DeviceContext> DeviceContext { get; private set; }

    /// <summary>
    /// Gets the optional surface creation options associated with this rendering session.
    /// </summary>
    public SurfaceCreationOptions? SurfaceCreationOptions { get; }

    /// <summary>
    /// Gets the optional rectangle of the surface being rendered to.
    /// </summary>
    public RECT? SurfaceRect { get; }

    /// <summary>
    /// Applies a transform to the device context for the duration of the specified action,
    /// then restores the previous transform even if an exception occurs.
    /// </summary>
    /// <param name="transform">The transform to apply.</param>
    /// <param name="action">The drawing action to execute under the given transform.</param>
    /// <param name="combineWithExisting">
    /// true to pre-multiply the new transform with the current device context transform; false to replace it.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
    public virtual void WithTransform(D2D_MATRIX_3X2_F transform, Action action, bool combineWithExisting = true)
    {
        ExceptionExtensions.ThrowIfNull(action, nameof(action));

        var existing = DeviceContext.GetTransform();
        if (combineWithExisting)
        {
            DeviceContext.SetTransform(existing * transform);
        }
        else
        {
            DeviceContext.SetTransform(transform);
        }
        try
        {
            action();
        }
        finally
        {
            DeviceContext.SetTransform(existing);
        }
    }

    /// <summary>
    /// Creates a temporary <see cref="RenderContext"/> for the specified device context and invokes the given action.
    /// The <see cref="DeviceContext"/> on the wrapper is invalidated after the action completes.
    /// </summary>
    /// <param name="deviceContext">The Direct2D device context to wrap.</param>
    /// <param name="action">The action to execute with the created <see cref="RenderContext"/>.</param>
    /// <param name="creationOptions">Optional surface creation options.</param>
    /// <param name="rect">Optional surface rectangle.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="deviceContext"/> or <paramref name="action"/> is null.</exception>
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
    /// <summary>
    /// Creates a solid color brush for the given color.
    /// </summary>
    /// <param name="color">The color to use for the brush; if null, returns null.</param>
    /// <returns>
    /// An <see cref="IComObject{T}"/> wrapping the created <see cref="ID2D1Brush"/>, or null if <paramref name="color"/> is null.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the device context has been invalidated.</exception>
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

    /// <summary>
    /// Creates a bitmap brush for the specified bitmap.
    /// </summary>
    /// <typeparam name="T">The bitmap brush interface type, e.g., <see cref="ID2D1BitmapBrush"/>.</typeparam>
    /// <param name="bitmap">The source bitmap.</param>
    /// <param name="bitmapBrushProperties">Optional bitmap brush properties.</param>
    /// <param name="brushProperties">Optional generic brush properties.</param>
    /// <returns>An <see cref="IComObject{T}"/> wrapping the created bitmap brush.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="bitmap"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the device context has been invalidated.</exception>
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

    /// <summary>
    /// Creates a linear gradient brush using the provided properties and gradient stops.
    /// </summary>
    /// <param name="properties">Linear gradient brush properties (start/end points, etc.).</param>
    /// <param name="stops">One or more gradient stops defining the gradient ramp.</param>
    /// <returns>An <see cref="IComObject{T}"/> wrapping the created <see cref="ID2D1Brush"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="stops"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the device context has been invalidated.</exception>
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

    /// <summary>
    /// Creates a radial gradient brush using the provided properties, gamma, extend mode, and gradient stops.
    /// </summary>
    /// <param name="properties">Radial gradient brush properties (center, radius, etc.).</param>
    /// <param name="gamma">The gamma for interpolation (default is 2.2).</param>
    /// <param name="extendMode">How the gradient extends beyond the [0,1] range (default is clamp).</param>
    /// <param name="stops">One or more gradient stops defining the gradient ramp.</param>
    /// <returns>An <see cref="IComObject{T}"/> wrapping the created <see cref="ID2D1Brush"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="stops"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the device context has been invalidated.</exception>
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
