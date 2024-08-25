namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1OpacityString)]
public partial class OpacityEffect : EffectWithSource
{
    public static EffectProperty OpacityProperty { get; }

    static OpacityEffect()
    {
        OpacityProperty = EffectProperty.Add(typeof(OpacityEffect), nameof(Opacity), 0, 1f);
    }

    public float Opacity { get => (float)GetPropertyValue(OpacityProperty)!; set => SetPropertyValue(OpacityProperty, value); }
}
