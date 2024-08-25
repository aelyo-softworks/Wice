namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1SharpenString)]
public partial class SharpenEffect : EffectWithSource
{
    public static EffectProperty SharpnessProperty { get; }
    public static EffectProperty ThresholdProperty { get; }

    static SharpenEffect()
    {
        SharpnessProperty = EffectProperty.Add(typeof(SharpenEffect), nameof(Sharpness), 0, 0f);
        ThresholdProperty = EffectProperty.Add(typeof(SharpenEffect), nameof(Threshold), 1, 0f);
    }

    public float Sharpness { get => (float)GetPropertyValue(SharpnessProperty)!; set => SetPropertyValue(SharpnessProperty, value.Clamp(0f, 10f)); }
    public float Threshold { get => (float)GetPropertyValue(ThresholdProperty)!; set => SetPropertyValue(ThresholdProperty, value.Clamp(0f, 1f)); }
}
