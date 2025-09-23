namespace Wice;

/// <summary>
/// A Direct2D bitmap brush wrapper that can be materialized for a given <see cref="RenderContext"/>.
/// </summary>
public class BitmapBrush : Brush
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BitmapBrush"/> class.
    /// </summary>
    /// <param name="bitmap">The source <see cref="ID2D1Bitmap"/> used by the brush. Must not be null.</param>
    /// <param name="bitmapBrushProperties">Optional bitmap-brush specific properties (extend modes, interpolation).</param>
    /// <param name="brushProperties">Optional generic brush properties (opacity, transform).</param>
    public BitmapBrush(ID2D1Bitmap bitmap, D2D1_BITMAP_BRUSH_PROPERTIES? bitmapBrushProperties = null, D2D1_BRUSH_PROPERTIES? brushProperties = null)
    {
        ExceptionExtensions.ThrowIfNull(bitmap, nameof(bitmap));
        Bitmap = bitmap;
        BitmapBrushProperties = bitmapBrushProperties;
        BrushProperties = brushProperties;
        Opacity = 1f;
        InterpolationMode = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR;
    }

    /// <summary>
    /// Gets the source <see cref="ID2D1Bitmap"/> used by this brush.
    /// </summary>
    public ID2D1Bitmap Bitmap { get; }

    /// <summary>
    /// Gets the optional bitmap-brush specific properties used at creation time.
    /// </summary>
    public D2D1_BITMAP_BRUSH_PROPERTIES? BitmapBrushProperties { get; }

    /// <summary>
    /// Gets the optional generic brush properties used at creation time.
    /// </summary>
    public D2D1_BRUSH_PROPERTIES? BrushProperties { get; }

    /// <summary>
    /// Gets or sets how the brush extends outside the [0,1] range along the X axis.
    /// Default is <see cref="D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP"/>.
    /// </summary>
    public D2D1_EXTEND_MODE ExtendModeX { get; set; }

    /// <summary>
    /// Gets or sets how the brush extends outside the [0,1] range along the Y axis.
    /// Default is <see cref="D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP"/>.
    /// </summary>
    public D2D1_EXTEND_MODE ExtendModeY { get; set; }

    /// <summary>
    /// Gets or sets the runtime opacity applied to the created brush. Default is 1.0.
    /// </summary>
    public float Opacity { get; set; }

    /// <summary>
    /// Gets or sets an optional transform applied to the created brush.
    /// </summary>
    public D2D_MATRIX_3X2_F? Transform { get; set; }

    /// <summary>
    /// Gets or sets the interpolation mode for sampling the bitmap. Default is linear.
    /// </summary>
    public D2D1_INTERPOLATION_MODE InterpolationMode { get; set; }

    /// <inheritdoc/>
    protected internal override IComObject<ID2D1Brush> GetBrush(RenderContext context)
    {
        // will be disposed by context
        var bmp = context.CreateBitmapBrush<ID2D1BitmapBrush1>(Bitmap, BitmapBrushProperties, BrushProperties);
        if (ExtendModeX != D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP)
        {
            bmp.Object.SetExtendModeX(ExtendModeX);
        }

        if (ExtendModeY != D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP)
        {
            bmp.Object.SetExtendModeY(ExtendModeY);
        }

        if (Opacity != 1)
        {
            bmp.Object.SetOpacity(Opacity);
        }

        if (Transform.HasValue)
        {
            var tx = Transform.Value;
            bmp.Object.SetTransform(tx);
        }

        if (InterpolationMode != D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR)
        {
            bmp.Object.SetInterpolationMode1(InterpolationMode);
        }
        return bmp;
    }

    /// <inheritdoc/>
    public override bool Equals(Brush? other)
    {
        if (other is not BitmapBrush brush)
            return false;

        if (!Bitmap.Equals(brush.Bitmap))
            return false;

        if (BitmapBrushProperties.HasValue)
        {
            if (!brush.BitmapBrushProperties.HasValue)
                return false;

            if (!BitmapBrushProperties.Value.Equals(brush.BitmapBrushProperties.Value))
                return false;
        }
        else if (brush.BitmapBrushProperties.HasValue)
            return false;

        if (BrushProperties.HasValue)
        {
            if (!brush.BrushProperties.HasValue)
                return false;

            if (!BrushProperties.Value.Equals(brush.BrushProperties.Value))
                return false;
        }
        else if (brush.BrushProperties.HasValue)
            return false;

        return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => Bitmap.GetHashCode() ^ BitmapBrushProperties.GetHashCode() ^ BrushProperties.GetHashCode();
}
