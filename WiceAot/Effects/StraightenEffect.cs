namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1StraightenString)]
public partial class StraightenEffect : EffectWithSource
{
    public static EffectProperty AngleProperty { get; }
    public static EffectProperty MaintainSizeProperty { get; }
    public static EffectProperty ScaleModeProperty { get; }

    static StraightenEffect()
    {
        AngleProperty = EffectProperty.Add(typeof(StraightenEffect), nameof(Angle), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
        MaintainSizeProperty = EffectProperty.Add(typeof(StraightenEffect), nameof(MaintainSize), 1, false);
        ScaleModeProperty = EffectProperty.Add(typeof(StraightenEffect), nameof(ScaleMode), 2, D2D1_STRAIGHTEN_SCALE_MODE.D2D1_STRAIGHTEN_SCALE_MODE_NEAREST_NEIGHBOR);
    }

    public float Angle { get => (float)GetPropertyValue(AngleProperty)!; set => SetPropertyValue(AngleProperty, value.Clamp(-45f, 45f)); }
    public bool MaintainSize { get => (bool)GetPropertyValue(MaintainSizeProperty)!; set => SetPropertyValue(MaintainSizeProperty, value); }
    public D2D1_STRAIGHTEN_SCALE_MODE ScaleMode { get => (D2D1_STRAIGHTEN_SCALE_MODE)GetPropertyValue(ScaleModeProperty)!; set => SetPropertyValue(ScaleModeProperty, value); }
}
