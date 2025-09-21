namespace Wice.Effects;

/// <summary>
/// Wraps the D2D built-in "RgbToHue" effect,
/// converting an RGB input into a hue-based color space.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1RgbToHueString)]
#else
[Guid(Constants.CLSID_D2D1RgbToHueString)]
#endif
public partial class RgbToHueEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for <see cref="OutputColorSpace"/> mapped at effect property index 0.
    /// </summary>
    public static EffectProperty OutputColorSpaceProperty { get; }

    /// <summary>
    /// Static initializer that registers property metadata for this effect.
    /// </summary>
    static RgbToHueEffect()
    {
        OutputColorSpaceProperty = EffectProperty.Add(
            typeof(RgbToHueEffect),
            nameof(OutputColorSpace),
            index: 0,
            D2D1_RGBTOHUE_OUTPUT_COLOR_SPACE.D2D1_RGBTOHUE_OUTPUT_COLOR_SPACE_HUE_SATURATION_VALUE
        );
    }

    /// <summary>
    /// Gets or sets the output color space produced by the effect.
    /// </summary>
    public D2D1_RGBTOHUE_OUTPUT_COLOR_SPACE OutputColorSpace
    {
        get => (D2D1_RGBTOHUE_OUTPUT_COLOR_SPACE)GetPropertyValue(OutputColorSpaceProperty)!;
        set => SetPropertyValue(OutputColorSpaceProperty, value);
    }
}
