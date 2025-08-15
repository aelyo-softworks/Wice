namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1ShadowString)]
#else
[Guid(Constants.CLSID_D2D1ShadowString)]
#endif
/// <summary>
/// Wraps the Direct2D Shadow effect.
/// </summary>
/// <remarks>
/// - Requires a primary input source (<see cref="EffectWithSource.Source"/>).
/// - Exposes the standard deviation for the blur kernel, the shadow color, and an optimization mode.
/// - Properties are registered with explicit effect parameter indices and mapping hints for D2D.
/// </remarks>
public partial class ShadowEffect : EffectWithSource
{
    /// <summary>
    /// Effect property descriptor for <see cref="BlurStandardDeviation"/>.
    /// </summary>
    /// <remarks>
    /// - Index: 0 (effect parameter index).
    /// - Mapping: <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT"/>.
    /// - Default: 3.0f.
    /// </remarks>
    public static EffectProperty BlurStandardDeviationProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="Color"/>.
    /// </summary>
    /// <remarks>
    /// - Index: 0 (effect parameter index).
    /// - Mapping: <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR4"/> (RGBA to Vector4).
    /// - Default: Black (A=1, R=0, G=0, B=0).
    /// </remarks>
    public static EffectProperty ColorProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="Optimization"/>.
    /// </summary>
    /// <remarks>
    /// - Index: 0 (effect parameter index).
    /// - Mapping: <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT"/>.
    /// - Default: <see cref="D2D1_SHADOW_OPTIMIZATION.D2D1_SHADOW_OPTIMIZATION_BALANCED"/>.
    /// </remarks>
    public static EffectProperty OptimizationProperty { get; }

    static ShadowEffect()
    {
        // Register effect property descriptors with their D2D parameter indices and mapping semantics.
        BlurStandardDeviationProperty = EffectProperty.Add(typeof(ShadowEffect), nameof(BlurStandardDeviation), 0, 3f);
        ColorProperty = EffectProperty.Add(typeof(ShadowEffect), nameof(Color), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR4, new D3DCOLORVALUE(1, 0, 0, 0));
        OptimizationProperty = EffectProperty.Add(typeof(ShadowEffect), nameof(Optimization), 0, D2D1_SHADOW_OPTIMIZATION.D2D1_SHADOW_OPTIMIZATION_BALANCED);
    }

    /// <summary>
    /// Gets or sets the Gaussian blur standard deviation (in DIPs) used by the shadow.
    /// </summary>
    public float BlurStandardDeviation { get => (float)GetPropertyValue(BlurStandardDeviationProperty)!; set => SetPropertyValue(BlurStandardDeviationProperty, value); }

    /// <summary>
    /// Gets or sets the shadow color.
    /// </summary>
    /// <remarks>
    /// The value is mapped to a Vector4 by the effect pipeline using COLOR_TO_VECTOR4 semantics.
    /// </remarks>
    public D3DCOLORVALUE Color { get => (D3DCOLORVALUE)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }

    /// <summary>
    /// Gets or sets the optimization mode that balances quality and performance for the shadow rendering.
    /// </summary>
    public D2D1_SHADOW_OPTIMIZATION Optimization { get => (D2D1_SHADOW_OPTIMIZATION)GetPropertyValue(OptimizationProperty)!; set => SetPropertyValue(OptimizationProperty, value); }
}
