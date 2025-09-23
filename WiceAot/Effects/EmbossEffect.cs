namespace Wice.Effects;

/// <summary>
/// Represents an emboss effect that applies a raised or recessed appearance to an image or visual.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1EmbossString)]
#else
[Guid(Constants.CLSID_D2D1EmbossString)]
#endif
public partial class EmbossEffect : EffectWithSource
{
    /// <summary>
    /// Gets the static property that represents the height parameter of the effect.
    /// </summary>
    public static EffectProperty HeightProperty { get; }

    /// <summary>
    /// Gets the property that represents the direction of the effect.
    /// </summary>
    public static EffectProperty DirectionProperty { get; }

    static EmbossEffect()
    {
        HeightProperty = EffectProperty.Add(typeof(EmbossEffect), nameof(Height), 0, 0.5f);
        DirectionProperty = EffectProperty.Add(typeof(EmbossEffect), nameof(Direction), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
    }

    /// <summary>
    /// Gets or sets the height value, constrained to a range between 0 and 10.
    /// </summary>
    public float Height { get => (float)GetPropertyValue(HeightProperty)!; set => SetPropertyValue(HeightProperty, value.Clamp(0f, 10f)); }

    /// <summary>
    /// Gets or sets the direction in degrees.
    /// </summary>
    public float Direction { get => (float)GetPropertyValue(DirectionProperty)!; set => SetPropertyValue(DirectionProperty, value.Clamp(0f, 360f)); }
}
