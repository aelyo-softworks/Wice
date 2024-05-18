namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1DisplacementMapString)]
public class DisplacementMapEffect : EffectWithSource
{
    public static EffectProperty ScaleProperty { get; }
    public static EffectProperty XChannelSelectProperty { get; }
    public static EffectProperty YChannelSelectProperty { get; }

    static DisplacementMapEffect()
    {
        ScaleProperty = EffectProperty.Add(typeof(DisplacementMapEffect), nameof(Scale), 0, 0f);
        XChannelSelectProperty = EffectProperty.Add(typeof(DisplacementMapEffect), nameof(XChannelSelect), 1, D2D1_CHANNEL_SELECTOR.D2D1_CHANNEL_SELECTOR_A);
        YChannelSelectProperty = EffectProperty.Add(typeof(DisplacementMapEffect), nameof(YChannelSelect), 2, D2D1_CHANNEL_SELECTOR.D2D1_CHANNEL_SELECTOR_A);
    }

    public DisplacementMapEffect()
            : base(2)
    {
    }

    public float Scale { get => (float)GetPropertyValue(ScaleProperty); set => SetPropertyValue(ScaleProperty, value); }
    public D2D1_CHANNEL_SELECTOR XChannelSelect { get => (D2D1_CHANNEL_SELECTOR)GetPropertyValue(XChannelSelectProperty); set => SetPropertyValue(XChannelSelectProperty, value); }
    public D2D1_CHANNEL_SELECTOR YChannelSelect { get => (D2D1_CHANNEL_SELECTOR)GetPropertyValue(YChannelSelectProperty); set => SetPropertyValue(YChannelSelectProperty, value); }

    public IGraphicsEffectSource Displacement { get => GetSource(1); set => SetSource(1, value); }
}
