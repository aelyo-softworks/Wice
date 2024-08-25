namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1GammaTransferString)]
public partial class GammaTransferEffect : EffectWithSource
{
    public static EffectProperty RedAmplitudeProperty { get; }
    public static EffectProperty RedExponentProperty { get; }
    public static EffectProperty RedOffsetProperty { get; }
    public static EffectProperty RedDisableProperty { get; }
    public static EffectProperty GreenAmplitudeProperty { get; }
    public static EffectProperty GreenExponentProperty { get; }
    public static EffectProperty GreenOffsetProperty { get; }
    public static EffectProperty GreenDisableProperty { get; }
    public static EffectProperty BlueAmplitudeProperty { get; }
    public static EffectProperty BlueExponentProperty { get; }
    public static EffectProperty BlueOffsetProperty { get; }
    public static EffectProperty BlueDisableProperty { get; }
    public static EffectProperty AlphaAmplitudeProperty { get; }
    public static EffectProperty AlphaExponentProperty { get; }
    public static EffectProperty AlphaOffsetProperty { get; }
    public static EffectProperty AlphaDisableProperty { get; }
    public static EffectProperty ClampOutputProperty { get; }

    static GammaTransferEffect()
    {
        RedAmplitudeProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(RedAmplitude), 0, 1f);
        RedExponentProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(RedExponent), 1, 1f);
        RedOffsetProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(RedOffset), 2, 0f);
        RedDisableProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(RedDisable), 3, false);
        GreenAmplitudeProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(GreenAmplitude), 4, 1f);
        GreenExponentProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(GreenExponent), 5, 1f);
        GreenOffsetProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(GreenOffset), 6, 0f);
        GreenDisableProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(GreenDisable), 7, false);
        BlueAmplitudeProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(BlueAmplitude), 8, 1f);
        BlueExponentProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(BlueExponent), 9, 1f);
        BlueOffsetProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(BlueOffset), 10, 0f);
        BlueDisableProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(BlueDisable), 11, false);
        AlphaAmplitudeProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(AlphaAmplitude), 12, 1f);
        AlphaExponentProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(AlphaExponent), 13, 1f);
        AlphaOffsetProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(AlphaOffset), 14, 0f);
        AlphaDisableProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(AlphaDisable), 15, false);
        ClampOutputProperty = EffectProperty.Add(typeof(GammaTransferEffect), nameof(ClampOutput), 16, false);
    }

    public float RedAmplitude { get => (float)GetPropertyValue(RedAmplitudeProperty)!; set => SetPropertyValue(RedAmplitudeProperty, value); }
    public float RedExponent { get => (float)GetPropertyValue(RedExponentProperty)!; set => SetPropertyValue(RedExponentProperty, value); }
    public float RedOffset { get => (float)GetPropertyValue(RedOffsetProperty)!; set => SetPropertyValue(RedOffsetProperty, value); }
    public bool RedDisable { get => (bool)GetPropertyValue(RedDisableProperty)!; set => SetPropertyValue(RedDisableProperty, value); }
    public float GreenAmplitude { get => (float)GetPropertyValue(GreenAmplitudeProperty)!; set => SetPropertyValue(GreenAmplitudeProperty, value); }
    public float GreenExponent { get => (float)GetPropertyValue(GreenExponentProperty)!; set => SetPropertyValue(GreenExponentProperty, value); }
    public float GreenOffset { get => (float)GetPropertyValue(GreenOffsetProperty)!; set => SetPropertyValue(GreenOffsetProperty, value); }
    public bool GreenDisable { get => (bool)GetPropertyValue(GreenDisableProperty)!; set => SetPropertyValue(GreenDisableProperty, value); }
    public float BlueAmplitude { get => (float)GetPropertyValue(BlueAmplitudeProperty)!; set => SetPropertyValue(BlueAmplitudeProperty, value); }
    public float BlueExponent { get => (float)GetPropertyValue(BlueExponentProperty)!; set => SetPropertyValue(BlueExponentProperty, value); }
    public float BlueOffset { get => (float)GetPropertyValue(BlueOffsetProperty)!; set => SetPropertyValue(BlueOffsetProperty, value); }
    public bool BlueDisable { get => (bool)GetPropertyValue(BlueDisableProperty)!; set => SetPropertyValue(BlueDisableProperty, value); }
    public float AlphaAmplitude { get => (float)GetPropertyValue(AlphaAmplitudeProperty)!; set => SetPropertyValue(AlphaAmplitudeProperty, value); }
    public float AlphaExponent { get => (float)GetPropertyValue(AlphaExponentProperty)!; set => SetPropertyValue(AlphaExponentProperty, value); }
    public float AlphaOffset { get => (float)GetPropertyValue(AlphaOffsetProperty)!; set => SetPropertyValue(AlphaOffsetProperty, value); }
    public bool AlphaDisable { get => (bool)GetPropertyValue(AlphaDisableProperty)!; set => SetPropertyValue(AlphaDisableProperty, value); }
    public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty)!; set => SetPropertyValue(ClampOutputProperty, value); }
}
