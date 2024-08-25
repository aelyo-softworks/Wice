namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1PointSpecularString)]
public partial class PointSpecularEffect : EffectWithSource
{
    public static EffectProperty LightPositionProperty { get; }
    public static EffectProperty SpecularExponentProperty { get; }
    public static EffectProperty SpecularConstantProperty { get; }
    public static EffectProperty SurfaceScaleProperty { get; }
    public static EffectProperty ColorProperty { get; }
    public static EffectProperty KernelUnitLengthProperty { get; }
    public static EffectProperty ScaleModeProperty { get; }

    static PointSpecularEffect()
    {
        LightPositionProperty = EffectProperty.Add(typeof(PointSpecularEffect), nameof(LightPosition), 0, new D2D_VECTOR_3F());
        SpecularExponentProperty = EffectProperty.Add(typeof(PointSpecularEffect), nameof(SpecularExponent), 1, 1f);
        SpecularConstantProperty = EffectProperty.Add(typeof(PointSpecularEffect), nameof(SpecularConstant), 2, 1f);
        SurfaceScaleProperty = EffectProperty.Add(typeof(PointSpecularEffect), nameof(SurfaceScale), 3, 1f);
        ColorProperty = EffectProperty.Add(typeof(PointSpecularEffect), nameof(Color), 4, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3, new D2D_VECTOR_3F(1f, 1f, 1f));
        KernelUnitLengthProperty = EffectProperty.Add(typeof(PointSpecularEffect), nameof(KernelUnitLength), 5, new D2D_VECTOR_2F(1f, 1f));
        ScaleModeProperty = EffectProperty.Add(typeof(PointSpecularEffect), nameof(ScaleMode), 6, D2D1_POINTSPECULAR_SCALE_MODE.D2D1_POINTSPECULAR_SCALE_MODE_LINEAR);
    }

    public D2D_VECTOR_3F LightPosition { get => (D2D_VECTOR_3F)GetPropertyValue(LightPositionProperty)!; set => SetPropertyValue(LightPositionProperty, value); }
    public float SpecularExponent { get => (float)GetPropertyValue(SpecularExponentProperty)!; set => SetPropertyValue(SpecularExponentProperty, value.Clamp(1f, 128f)); }
    public float SpecularConstant { get => (float)GetPropertyValue(SpecularConstantProperty)!; set => SetPropertyValue(SpecularConstantProperty, value.Clamp(0f, 10000f)); }
    public float SurfaceScale { get => (float)GetPropertyValue(SurfaceScaleProperty)!; set => SetPropertyValue(SurfaceScaleProperty, value.Clamp(0f, 10000f)); }
    public D2D_VECTOR_3F Color { get => (D2D_VECTOR_3F)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }
    public D2D_VECTOR_2F KernelUnitLength { get => (D2D_VECTOR_2F)GetPropertyValue(KernelUnitLengthProperty)!; set => SetPropertyValue(KernelUnitLengthProperty, value); }
    public D2D1_POINTSPECULAR_SCALE_MODE ScaleMode { get => (D2D1_POINTSPECULAR_SCALE_MODE)GetPropertyValue(ScaleModeProperty)!; set => SetPropertyValue(ScaleModeProperty, value); }
}
