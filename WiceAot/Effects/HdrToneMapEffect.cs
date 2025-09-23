namespace Wice.Effects;

/// <summary>
/// Represents an effect that applies high dynamic range (HDR) tone mapping to an image.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1HdrToneMapString)]
#else
[Guid(Constants.CLSID_D2D1HdrToneMapString)]
#endif
public partial class HdrToneMapEffect : EffectWithSource
{
    /// <summary>
    /// Gets the effect property that specifies the maximum luminance value for the input.
    /// </summary>
    public static EffectProperty InputMaxLuminanceProperty { get; }

    /// <summary>
    /// Gets the effect property that specifies the maximum luminance value for the output.
    /// </summary>
    public static EffectProperty OutputMaxLuminanceProperty { get; }

    /// <summary>
    /// Gets the static property that represents the display mode configuration for the effect.
    /// </summary>
    public static EffectProperty DisplayModeProperty { get; }

    static HdrToneMapEffect()
    {
        InputMaxLuminanceProperty = EffectProperty.Add(typeof(HdrToneMapEffect), nameof(InputMaxLuminance), 0, 0f);
        OutputMaxLuminanceProperty = EffectProperty.Add(typeof(HdrToneMapEffect), nameof(OutputMaxLuminance), 1, 0f);
        DisplayModeProperty = EffectProperty.Add(typeof(HdrToneMapEffect), nameof(DisplayMode), 2, D2D1_HDRTONEMAP_DISPLAY_MODE.D2D1_HDRTONEMAP_DISPLAY_MODE_SDR);
    }

    /// <summary>
    /// Gets or sets the maximum luminance value for the input.
    /// </summary>
    public float InputMaxLuminance { get => (float)GetPropertyValue(InputMaxLuminanceProperty)!; set => SetPropertyValue(InputMaxLuminanceProperty, value); }

    /// <summary>
    /// Gets or sets the maximum luminance value for the output display.
    /// </summary>
    public float OutputMaxLuminance { get => (float)GetPropertyValue(OutputMaxLuminanceProperty)!; set => SetPropertyValue(OutputMaxLuminanceProperty, value); }

    /// <summary>
    /// Gets or sets the display mode used for HDR tone mapping.
    /// </summary>
    public D2D1_HDRTONEMAP_DISPLAY_MODE DisplayMode { get => (D2D1_HDRTONEMAP_DISPLAY_MODE)GetPropertyValue(DisplayModeProperty)!; set => SetPropertyValue(DisplayModeProperty, value); }
}
