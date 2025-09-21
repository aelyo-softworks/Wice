namespace Wice.Effects;

/// <summary>
/// Represents an effect that converts hue-based color data to RGB color data.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1HueToRgbString)]
#else
[Guid(Constants.CLSID_D2D1HueToRgbString)]
#endif
public partial class HueToRgbEffect : EffectWithSource
{
    /// <summary>
    /// Gets the property that specifies the input color space for the effect.
    /// </summary>
    public static EffectProperty InputColorSpaceProperty { get; }

    /// <summary>
    /// Initializes static members of the <see cref="HueToRgbEffect"/> class.
    /// </summary>
    static HueToRgbEffect()
    {
        InputColorSpaceProperty = EffectProperty.Add(typeof(HueToRgbEffect), nameof(InputColorSpace), 0, D2D1_HUETORGB_INPUT_COLOR_SPACE.D2D1_HUETORGB_INPUT_COLOR_SPACE_HUE_SATURATION_VALUE);
    }

    /// <summary>
    /// Gets or sets the input color space used for the Hue-to-RGB conversion.
    /// </summary>
    public D2D1_HUETORGB_INPUT_COLOR_SPACE InputColorSpace { get => (D2D1_HUETORGB_INPUT_COLOR_SPACE)GetPropertyValue(InputColorSpaceProperty)!; set => SetPropertyValue(InputColorSpaceProperty, value); }
}
