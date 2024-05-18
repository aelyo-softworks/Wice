namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1HueRotationString)]
public class HueRotationEffect : EffectWithSource
{
    public static EffectProperty AngleProperty { get; }

    static HueRotationEffect()
    {
        AngleProperty = EffectProperty.Add(typeof(HueRotationEffect), nameof(Angle), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
    }

    public float Angle { get => (float)GetPropertyValue(AngleProperty); set => SetPropertyValue(AngleProperty, value); }
}
