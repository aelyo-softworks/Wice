namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1FloodString)]
public class FloodEffect : Effect
{
    public static EffectProperty ColorProperty { get; }

    static FloodEffect()
    {
        ColorProperty = EffectProperty.Add(typeof(FloodEffect), nameof(Color), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR4, new D2D_VECTOR_4F(0f, 0f, 0f, 1f));
    }

    public D2D_VECTOR_4F Color { get => (D2D_VECTOR_4F)GetPropertyValue(ColorProperty); set => SetPropertyValue(ColorProperty, value); }
}
