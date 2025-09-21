namespace Wice.Effects;

/// <summary>
/// Wraps the Direct2D Vignette effect.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1VignetteString)]
#else
[Guid(Constants.CLSID_D2D1VignetteString)]
#endif
public partial class VignetteEffect : EffectWithSource
{
    /// <summary>
    /// Effect metadata for <see cref="Color"/>.
    /// </summary>
    public static EffectProperty ColorProperty { get; }

    /// <summary>
    /// Effect metadata for <see cref="TransitionSize"/>.
    /// </summary>
    public static EffectProperty TransitionSizeProperty { get; }

    /// <summary>
    /// Effect metadata for <see cref="Strength"/>.
    /// </summary>
    public static EffectProperty StrengthProperty { get; }

    /// <summary>
    /// Registers effect properties with the global property registry and freezes their metadata.
    /// </summary>
    static VignetteEffect()
    {
        ColorProperty = EffectProperty.Add(typeof(VignetteEffect), nameof(Color), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR4, D3DCOLORVALUE.Black);
        TransitionSizeProperty = EffectProperty.Add(typeof(VignetteEffect), nameof(TransitionSize), 1, 0f);
        StrengthProperty = EffectProperty.Add(typeof(VignetteEffect), nameof(Strength), 2, 0f);
    }

    /// <summary>
    /// Gets or sets the tint color applied by the vignette.
    /// </summary>
    public D3DCOLORVALUE Color { get => (D3DCOLORVALUE)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }

    /// <summary>
    /// Gets or sets the size of the smooth transition from the center to the darkened edges.
    /// </summary>
    public float TransitionSize { get => (float)GetPropertyValue(TransitionSizeProperty)!; set => SetPropertyValue(TransitionSizeProperty, value); }

    /// <summary>
    /// Gets or sets the strength of the vignette darkening.
    /// </summary>
    public float Strength { get => (float)GetPropertyValue(StrengthProperty)!; set => SetPropertyValue(StrengthProperty, value); }
}
