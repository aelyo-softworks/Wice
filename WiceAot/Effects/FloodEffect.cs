namespace Wice.Effects;

/// <summary>
/// Represents an effect that fills the output with a solid color.
/// </summary>
/// <remarks>The <see cref="FloodEffect"/> is used to generate a uniform color output, which can be applied as a
/// base or combined with other effects in a graphics pipeline.</remarks>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1FloodString)]
#else
[Guid(Constants.CLSID_D2D1FloodString)]
#endif
public partial class FloodEffect : Effect
{
    /// <summary>
    /// Gets the effect property that represents the color used in the effect.
    /// </summary>
    public static EffectProperty ColorProperty { get; }

    /// <summary>
    /// Initializes static members of the <see cref="FloodEffect"/> class.
    /// </summary>
    /// <remarks>This static constructor sets up the <see cref="ColorProperty"/> for the <see
    /// cref="FloodEffect"/> class, mapping the color property to a vector representation with a default value of black
    /// (RGBA: 0, 0, 0, 1).</remarks>
    static FloodEffect()
    {
        ColorProperty = EffectProperty.Add(typeof(FloodEffect), nameof(Color), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR4, new D2D_VECTOR_4F(0f, 0f, 0f, 1f));
    }

    /// <summary>
    /// Gets or sets the color value associated with this property.
    /// </summary>
    public D2D_VECTOR_4F Color { get => (D2D_VECTOR_4F)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }
}
