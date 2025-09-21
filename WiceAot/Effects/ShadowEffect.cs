namespace Wice.Effects;

/// <summary>
/// Wraps the Direct2D Shadow effect.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1ShadowString)]
#else
[Guid(Constants.CLSID_D2D1ShadowString)]
#endif
public partial class ShadowEffect : EffectWithSource
{
    /// <summary>
    /// Effect property descriptor for <see cref="BlurStandardDeviation"/>.
    /// </summary>
    public static EffectProperty BlurStandardDeviationProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="Color"/>.
    /// </summary>
    public static EffectProperty ColorProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="Optimization"/>.
    /// </summary>
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
    public D3DCOLORVALUE Color { get => (D3DCOLORVALUE)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }

    /// <summary>
    /// Gets or sets the optimization mode that balances quality and performance for the shadow rendering.
    /// </summary>
    public D2D1_SHADOW_OPTIMIZATION Optimization { get => (D2D1_SHADOW_OPTIMIZATION)GetPropertyValue(OptimizationProperty)!; set => SetPropertyValue(OptimizationProperty, value); }
}
