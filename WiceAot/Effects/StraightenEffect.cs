namespace Wice.Effects;

/// <summary>
/// Wraps the Direct2D Straighten effect.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1StraightenString)]
#else
[Guid(Constants.CLSID_D2D1StraightenString)]
#endif
public partial class StraightenEffect : EffectWithSource
{
    /// <summary>
    /// Effect property descriptor for <see cref="Angle"/> at index 0.
    /// </summary>
    public static EffectProperty AngleProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="MaintainSize"/> at index 1.
    /// </summary>
    public static EffectProperty MaintainSizeProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="ScaleMode"/> at index 2.
    /// </summary>
    public static EffectProperty ScaleModeProperty { get; }

    /// <summary>
    /// Initializes and registers effect property descriptors.
    /// </summary>
    static StraightenEffect()
    {
        AngleProperty = EffectProperty.Add(typeof(StraightenEffect), nameof(Angle), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
        MaintainSizeProperty = EffectProperty.Add(typeof(StraightenEffect), nameof(MaintainSize), 1, false);
        ScaleModeProperty = EffectProperty.Add(typeof(StraightenEffect), nameof(ScaleMode), 2, D2D1_STRAIGHTEN_SCALE_MODE.D2D1_STRAIGHTEN_SCALE_MODE_NEAREST_NEIGHBOR);
    }

    /// <summary>
    /// Gets or sets the straighten rotation angle.
    /// </summary>
    public float Angle { get => (float)GetPropertyValue(AngleProperty)!; set => SetPropertyValue(AngleProperty, value.Clamp(-45f, 45f)); }

    /// <summary>
    /// Gets or sets whether the effect maintains the original output size.
    /// </summary>
    public bool MaintainSize { get => (bool)GetPropertyValue(MaintainSizeProperty)!; set => SetPropertyValue(MaintainSizeProperty, value); }

    /// <summary>
    /// Gets or sets the resampling mode used when scaling is applied by the effect.
    /// </summary>
    public D2D1_STRAIGHTEN_SCALE_MODE ScaleMode { get => (D2D1_STRAIGHTEN_SCALE_MODE)GetPropertyValue(ScaleModeProperty)!; set => SetPropertyValue(ScaleModeProperty, value); }
}
