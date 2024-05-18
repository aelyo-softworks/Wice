namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1ScaleString)]
public class ScaleEffect : EffectWithSource
{
    public static EffectProperty ScaleProperty { get; }
    public static EffectProperty CenterPointProperty { get; }
    public static EffectProperty InterpolationModeProperty { get; }
    public static EffectProperty BorderModeProperty { get; }
    public static EffectProperty SharpnessProperty { get; }

    static ScaleEffect()
    {
        ScaleProperty = EffectProperty.Add(typeof(ScaleEffect), nameof(Scale), 0, new D2D_VECTOR_2F(1f, 1f));
        CenterPointProperty = EffectProperty.Add(typeof(ScaleEffect), nameof(CenterPoint), 1, new D2D_VECTOR_2F(0f, 0f));
        InterpolationModeProperty = EffectProperty.Add(typeof(ScaleEffect), nameof(InterpolationMode), 2, D2D1_SCALE_INTERPOLATION_MODE.D2D1_SCALE_INTERPOLATION_MODE_LINEAR);
        BorderModeProperty = EffectProperty.Add(typeof(ScaleEffect), nameof(BorderMode), 3, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
        SharpnessProperty = EffectProperty.Add(typeof(ScaleEffect), nameof(Sharpness), 4, 0f);
    }

    public D2D_VECTOR_2F Scale { get => (D2D_VECTOR_2F)GetPropertyValue(ScaleProperty); set => SetPropertyValue(ScaleProperty, value); }
    public D2D_VECTOR_2F CenterPoint { get => (D2D_VECTOR_2F)GetPropertyValue(CenterPointProperty); set => SetPropertyValue(CenterPointProperty, value); }
    public D2D1_SCALE_INTERPOLATION_MODE InterpolationMode { get => (D2D1_SCALE_INTERPOLATION_MODE)GetPropertyValue(InterpolationModeProperty); set => SetPropertyValue(InterpolationModeProperty, value); }
    public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty); set => SetPropertyValue(BorderModeProperty, value); }
    public float Sharpness { get => (float)GetPropertyValue(SharpnessProperty); set => SetPropertyValue(SharpnessProperty, value.Clamp(0f, 1f)); }
}
