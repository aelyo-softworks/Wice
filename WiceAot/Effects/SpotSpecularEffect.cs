namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1SpotSpecularString)]
public partial class SpotSpecularEffect : EffectWithSource
{
    public static EffectProperty LightPositionProperty { get; }
    public static EffectProperty PointsAtProperty { get; }
    public static EffectProperty FocusProperty { get; }
    public static EffectProperty LimitingConeAngleProperty { get; }
    public static EffectProperty SpecularExponentProperty { get; }
    public static EffectProperty SpecularConstantProperty { get; }
    public static EffectProperty SurfaceScaleProperty { get; }
    public static EffectProperty ColorProperty { get; }
    public static EffectProperty KernelUnitLengthProperty { get; }
    public static EffectProperty ScaleModeProperty { get; }

    static SpotSpecularEffect()
    {
        LightPositionProperty = EffectProperty.Add(typeof(SpotDiffuseEffect), nameof(LightPosition), 0, new D2D_VECTOR_3F());
        PointsAtProperty = EffectProperty.Add(typeof(SpotDiffuseEffect), nameof(PointsAt), 1, new D2D_VECTOR_3F());
        FocusProperty = EffectProperty.Add(typeof(SpotDiffuseEffect), nameof(Focus), 2, 1f);
        LimitingConeAngleProperty = EffectProperty.Add(typeof(SpotDiffuseEffect), nameof(LimitingConeAngle), 3, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 90f);
        SpecularExponentProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(SpecularExponent), 4, 1f);
        SpecularConstantProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(SpecularConstant), 5, 1f);
        SurfaceScaleProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(SurfaceScale), 6, 1f);
        ColorProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(Color), 7, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3, new D2D_VECTOR_3F(1f, 1f, 1f));
        KernelUnitLengthProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(KernelUnitLength), 8, new D2D_VECTOR_2F(1f, 1f));
        ScaleModeProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(ScaleMode), 9, D2D1_POINTDIFFUSE_SCALE_MODE.D2D1_POINTDIFFUSE_SCALE_MODE_LINEAR);
    }

    public D2D_VECTOR_3F LightPosition { get => (D2D_VECTOR_3F)GetPropertyValue(LightPositionProperty)!; set => SetPropertyValue(LightPositionProperty, value); }
    public D2D_VECTOR_3F PointsAt { get => (D2D_VECTOR_3F)GetPropertyValue(PointsAtProperty)!; set => SetPropertyValue(PointsAtProperty, value); }
    public float Focus { get => (float)GetPropertyValue(FocusProperty)!; set => SetPropertyValue(FocusProperty, value.Clamp(0f, 200f)); }
    public float LimitingConeAngle { get => (float)GetPropertyValue(LimitingConeAngleProperty)!; set => SetPropertyValue(LimitingConeAngleProperty, value.Clamp(0f, 90f)); }
    public float SpecularExponent { get => (float)GetPropertyValue(SpecularExponentProperty)!; set => SetPropertyValue(SpecularExponentProperty, value.Clamp(1f, 128f)); }
    public float SpecularConstant { get => (float)GetPropertyValue(SpecularConstantProperty)!; set => SetPropertyValue(SpecularConstantProperty, value.Clamp(0f, 10000f)); }
    public float SurfaceScale { get => (float)GetPropertyValue(SurfaceScaleProperty)!; set => SetPropertyValue(SurfaceScaleProperty, value.Clamp(0f, 10000f)); }
    public D2D_VECTOR_3F Color { get => (D2D_VECTOR_3F)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }
    public D2D_VECTOR_2F KernelUnitLength { get => (D2D_VECTOR_2F)GetPropertyValue(KernelUnitLengthProperty)!; set => SetPropertyValue(KernelUnitLengthProperty, value); }
    public D2D1_POINTDIFFUSE_SCALE_MODE ScaleMode { get => (D2D1_POINTDIFFUSE_SCALE_MODE)GetPropertyValue(ScaleModeProperty)!; set => SetPropertyValue(ScaleModeProperty, value); }
}
