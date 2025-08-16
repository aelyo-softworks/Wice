namespace Wice.Effects;

/// <summary>
/// Represents an effect that converts hue-based color data to RGB color data.
/// </summary>
/// <remarks>The <see cref="HueToRgbEffect"/> class is used to transform color data from a hue-based color space 
/// (such as Hue-Saturation-Value) into the RGB color space. This effect is commonly used in image  processing scenarios
/// where color manipulation or conversion is required.</remarks>
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
    /// <remarks>This property is typically used to define or retrieve the color space in which the input data
    /// is processed. Ensure that the value is compatible with the effect's requirements to avoid unexpected
    /// behavior.</remarks>
    public static EffectProperty InputColorSpaceProperty { get; }

    /// <summary>
    /// Initializes static members of the <see cref="HueToRgbEffect"/> class.
    /// </summary>
    /// <remarks>This static constructor sets up the <see cref="InputColorSpaceProperty"/> for the <see
    /// cref="HueToRgbEffect"/> class, defining the default input color space as <see
    /// cref="D2D1_HUETORGB_INPUT_COLOR_SPACE.D2D1_HUETORGB_INPUT_COLOR_SPACE_HUE_SATURATION_VALUE"/>.</remarks>
    static HueToRgbEffect()
    {
        InputColorSpaceProperty = EffectProperty.Add(typeof(HueToRgbEffect), nameof(InputColorSpace), 0, D2D1_HUETORGB_INPUT_COLOR_SPACE.D2D1_HUETORGB_INPUT_COLOR_SPACE_HUE_SATURATION_VALUE);
    }

    /// <summary>
    /// Gets or sets the input color space used for the Hue-to-RGB conversion.
    /// </summary>
    /// <remarks>This property determines the color space of the input data for the Hue-to-RGB conversion
    /// process. Ensure that the value is compatible with the expected input format of the operation.</remarks>
    public D2D1_HUETORGB_INPUT_COLOR_SPACE InputColorSpace { get => (D2D1_HUETORGB_INPUT_COLOR_SPACE)GetPropertyValue(InputColorSpaceProperty)!; set => SetPropertyValue(InputColorSpaceProperty, value); }
}
