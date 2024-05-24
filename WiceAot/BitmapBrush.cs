namespace Wice;

public class BitmapBrush : Brush
{
    public BitmapBrush(ID2D1Bitmap bitmap, D2D1_BITMAP_BRUSH_PROPERTIES? bitmapBrushProperties = null, D2D1_BRUSH_PROPERTIES? brushProperties = null)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        Bitmap = bitmap;
        BitmapBrushProperties = bitmapBrushProperties;
        BrushProperties = brushProperties;
        Opacity = 1f;
        InterpolationMode = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR;
    }

    public ID2D1Bitmap Bitmap { get; }
    public D2D1_BITMAP_BRUSH_PROPERTIES? BitmapBrushProperties { get; }
    public D2D1_BRUSH_PROPERTIES? BrushProperties { get; }
    public D2D1_EXTEND_MODE ExtendModeX { get; set; }
    public D2D1_EXTEND_MODE ExtendModeY { get; set; }
    public float Opacity { get; set; }
    public D2D_MATRIX_3X2_F? Transform { get; set; }
    public D2D1_INTERPOLATION_MODE InterpolationMode { get; set; }

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

    public override int GetHashCode() => Bitmap.GetHashCode() ^ BitmapBrushProperties.GetHashCode() ^ BrushProperties.GetHashCode();
}
