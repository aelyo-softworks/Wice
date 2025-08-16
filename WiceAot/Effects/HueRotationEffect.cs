namespace Wice.Effects;

/// <summary>
/// Represents an effect that rotates the hue of an image by a specified angle.
/// </summary>
/// <remarks>The <see cref="HueRotationEffect"/> applies a hue rotation to the input image, altering the colors by
/// shifting their hue values. The rotation is specified in radians and is converted internally to degrees.</remarks>
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
    /// <remarks>This static constructor sets up the <see cref="AngleProperty"/> for the <see
    /// cref="HueRotationEffect"/> class, defining its default value, property mapping, and other metadata.</remarks>
    static HueRotationEffect()
    {
        AngleProperty = EffectProperty.Add(typeof(HueRotationEffect), nameof(Angle), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
    }

    /// <summary>
    /// Gets or sets the angle value associated with this instance.
    /// </summary>
    /// <remarks>The angle is typically used to represent a rotational or directional value. Ensure that the
    /// value is within the expected range for the specific context in which it is used.</remarks>
    public float Angle { get => (float)GetPropertyValue(AngleProperty)!; set => SetPropertyValue(AngleProperty, value); }
}
