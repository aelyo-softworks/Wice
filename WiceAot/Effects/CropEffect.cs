namespace Wice.Effects;

/// <summary>
/// Direct2D Crop effect wrapper.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1CropString)]
#else
[Guid(Constants.CLSID_D2D1CropString)]
#endif
public partial class CropEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for <see cref="Rect"/> (effect parameter index 0).
    /// </summary>
    public static EffectProperty RectProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="BorderMode"/> (effect parameter index 1).
    /// </summary>
    public static EffectProperty BorderModeProperty { get; }

    static CropEffect()
    {
        RectProperty = EffectProperty.Add(typeof(CropEffect), nameof(Rect), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4, new D2D_VECTOR_4F(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue));
        BorderModeProperty = EffectProperty.Add(typeof(CropEffect), nameof(BorderMode), 1, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
    }

    /// <summary>
    /// Gets or sets the crop rectangle in DIPs, expressed as (Left, Top, Right, Bottom).
    /// </summary>
    public D2D_VECTOR_4F Rect { get => (D2D_VECTOR_4F)GetPropertyValue(RectProperty)!; set => SetPropertyValue(RectProperty, value); }

    /// <summary>
    /// Gets or sets how the effect samples pixels at and beyond the crop boundary.
    /// </summary>
    public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty)!; set => SetPropertyValue(BorderModeProperty, value); }
}
