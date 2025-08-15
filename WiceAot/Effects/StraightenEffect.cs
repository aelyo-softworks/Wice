namespace Wice.Effects;

/// <summary>
/// Wraps the Direct2D Straighten effect.
/// </summary>
/// <remarks>
/// - Exposes three effect properties mapped to the underlying D2D effect:
///   - <see cref="Angle"/> (index 0): rotation angle, clamped to [-45, 45].
///   - <see cref="MaintainSize"/> (index 1): whether to maintain the original output size.
///   - <see cref="ScaleMode"/> (index 2): resampling mode when scaling is applied.
/// - The CLSID is provided via the <see cref="GuidAttribute"/> for D2D interop.
/// - Inherits from <see cref="EffectWithSource"/>, requiring at least one input source.
/// </remarks>
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
    /// <remarks>
    /// Uses <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES"/> mapping.
    /// Default: <c>0f</c>.
    /// </remarks>
    public static EffectProperty AngleProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="MaintainSize"/> at index 1.
    /// </summary>
    /// <remarks>
    /// Default: <see langword="false"/>.
    /// </remarks>
    public static EffectProperty MaintainSizeProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="ScaleMode"/> at index 2.
    /// </summary>
    /// <remarks>
    /// Default: <see cref="D2D1_STRAIGHTEN_SCALE_MODE.D2D1_STRAIGHTEN_SCALE_MODE_NEAREST_NEIGHBOR"/>.
    /// </remarks>
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
    /// <remarks>
    /// - Range: [-45, 45]. Values outside this range are clamped.
    /// - Default: <c>0</c>.
    /// </remarks>
    public float Angle { get => (float)GetPropertyValue(AngleProperty)!; set => SetPropertyValue(AngleProperty, value.Clamp(-45f, 45f)); }

    /// <summary>
    /// Gets or sets whether the effect maintains the original output size.
    /// </summary>
    /// <remarks>
    /// When enabled, the effect may scale the content to avoid empty regions introduced by rotation.
    /// Default: <see langword="false"/>.
    /// </remarks>
    public bool MaintainSize { get => (bool)GetPropertyValue(MaintainSizeProperty)!; set => SetPropertyValue(MaintainSizeProperty, value); }

    /// <summary>
    /// Gets or sets the resampling mode used when scaling is applied by the effect.
    /// </summary>
    /// <remarks>
    /// Default: <see cref="D2D1_STRAIGHTEN_SCALE_MODE.D2D1_STRAIGHTEN_SCALE_MODE_NEAREST_NEIGHBOR"/>.
    /// </remarks>
    public D2D1_STRAIGHTEN_SCALE_MODE ScaleMode { get => (D2D1_STRAIGHTEN_SCALE_MODE)GetPropertyValue(ScaleModeProperty)!; set => SetPropertyValue(ScaleModeProperty, value); }
}
