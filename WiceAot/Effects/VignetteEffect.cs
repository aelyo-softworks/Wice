namespace Wice.Effects;

/// <summary>
/// Wraps the Direct2D Vignette effect.
/// </summary>
/// <remarks>
/// - Exposes three parameters mapped to the underlying D2D effect:
///   0: <see cref="Color"/> (mapped with <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR4"/>)
///   1: <see cref="TransitionSize"/>
///   2: <see cref="Strength"/>
/// - This effect requires a source image (<see cref="EffectWithSource.Source"/>).
/// </remarks>
/// <seealso cref="EffectWithSource"/>
/// <seealso cref="EffectProperty"/>
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
    /// <remarks>
    /// Index: 0<br/>
    /// Mapping: <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR4"/> (RGBA normalized vector)<br/>
    /// Default: <see cref="D3DCOLORVALUE.Black"/>
    /// </remarks>
    public static EffectProperty ColorProperty { get; }

    /// <summary>
    /// Effect metadata for <see cref="TransitionSize"/>.
    /// </summary>
    /// <remarks>
    /// Index: 1<br/>
    /// Mapping: <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT"/> (float)<br/>
    /// Default: 0f
    /// </remarks>
    public static EffectProperty TransitionSizeProperty { get; }

    /// <summary>
    /// Effect metadata for <see cref="Strength"/>.
    /// </summary>
    /// <remarks>
    /// Index: 2<br/>
    /// Mapping: <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT"/> (float)<br/>
    /// Default: 0f
    /// </remarks>
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
    /// <remarks>
    /// - Mapped as a normalized vector4 (R,G,BA).<br/>
    /// - Default is black (<see cref="D3DCOLORVALUE.Black"/>), which produces a dark vignette.<br/>
    /// - Use non-black colors to create colored vignettes.
    /// </remarks>
    public D3DCOLORVALUE Color { get => (D3DCOLORVALUE)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }

    /// <summary>
    /// Gets or sets the size of the smooth transition from the center to the darkened edges.
    /// </summary>
    /// <remarks>
    /// - Typical range is [0, 1].<br/>
    /// - 0 produces a sharp edge; higher values increase the feathering width.<br/>
    /// - Default is 0.
    /// </remarks>
    public float TransitionSize { get => (float)GetPropertyValue(TransitionSizeProperty)!; set => SetPropertyValue(TransitionSizeProperty, value); }

    /// <summary>
    /// Gets or sets the strength of the vignette darkening.
    /// </summary>
    /// <remarks>
    /// - Typical range is [0, 1].<br/>
    /// - 0 disables the effect; 1 applies maximum intensity.<br/>
    /// - Default is 0.
    /// </remarks>
    public float Strength { get => (float)GetPropertyValue(StrengthProperty)!; set => SetPropertyValue(StrengthProperty, value); }
}
