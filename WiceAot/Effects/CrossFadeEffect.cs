namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1CrossFadeString)]
public partial class CrossFadeEffect : EffectWithTwoSources
{
    public static EffectProperty WeightProperty { get; }

    static CrossFadeEffect()
    {
        WeightProperty = EffectProperty.Add(typeof(CrossFadeEffect), nameof(Weight), 0, 0.5f);
    }

    public float Weight { get => (float)GetPropertyValue(WeightProperty)!; set => SetPropertyValue(WeightProperty, value.Clamp(-0f, 1f)); }
}
