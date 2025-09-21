namespace Wice.Effects;

/// <summary>
/// Represents an effect that rotates the hue of an image by a specified angle.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1HueRotationString)]
#else
[Guid(Constants.CLSID_D2D1HueRotationString)]
#endif
public partial class HueRotationEffect : EffectWithSource
{
    /// <summary>
    /// Gets the dependency property that represents the angle of the effect.
    /// </summary>
    public static EffectProperty AngleProperty { get; }

    /// <summary>
    /// Initializes static members of the <see cref="HueRotationEffect"/> class.
    /// </summary>
    static HueRotationEffect()
    {
        AngleProperty = EffectProperty.Add(typeof(HueRotationEffect), nameof(Angle), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
    }

    /// <summary>
    /// Gets or sets the angle value associated with this instance.
    /// </summary>
    public float Angle { get => (float)GetPropertyValue(AngleProperty)!; set => SetPropertyValue(AngleProperty, value); }
}
