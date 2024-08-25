namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1TurbulenceString)]
public partial class TurbulenceEffect : Effect
{
    public static EffectProperty OffsetProperty { get; }
    public static EffectProperty SizeProperty { get; }
    public static EffectProperty BaseFrequencyProperty { get; }
    public static EffectProperty NumOctavesProperty { get; }
    public static EffectProperty SeedProperty { get; }
    public static EffectProperty NoiseProperty { get; }
    public static EffectProperty StitchableProperty { get; }

    static TurbulenceEffect()
    {
        OffsetProperty = EffectProperty.Add(typeof(TurbulenceEffect), nameof(Offset), 0, new D2D_VECTOR_2F());
        SizeProperty = EffectProperty.Add(typeof(TurbulenceEffect), nameof(Size), 1, new D2D_VECTOR_2F());
        BaseFrequencyProperty = EffectProperty.Add(typeof(TurbulenceEffect), nameof(BaseFrequency), 2, new D2D_VECTOR_2F(0.01f, 0.01f));
        NumOctavesProperty = EffectProperty.Add(typeof(TurbulenceEffect), nameof(NumOctaves), 3, 1u);
        SeedProperty = EffectProperty.Add(typeof(TurbulenceEffect), nameof(Seed), 4, 1u);
        NoiseProperty = EffectProperty.Add(typeof(TurbulenceEffect), nameof(Noise), 5, D2D1_TURBULENCE_NOISE.D2D1_TURBULENCE_NOISE_FRACTAL_SUM);
        StitchableProperty = EffectProperty.Add(typeof(TurbulenceEffect), nameof(Stitchable), 6, false);
    }

    public D2D_VECTOR_2F Offset { get => (D2D_VECTOR_2F)GetPropertyValue(OffsetProperty)!; set => SetPropertyValue(OffsetProperty, value); }
    public D2D_VECTOR_2F Size { get => (D2D_VECTOR_2F)GetPropertyValue(SizeProperty)!; set => SetPropertyValue(SizeProperty, value); }
    public D2D_VECTOR_2F BaseFrequency { get => (D2D_VECTOR_2F)GetPropertyValue(BaseFrequencyProperty)!; set => SetPropertyValue(BaseFrequencyProperty, value); }
    public uint NumOctaves { get => (uint)GetPropertyValue(NumOctavesProperty)!; set => SetPropertyValue(NumOctavesProperty, value); }
    public uint Seed { get => (uint)GetPropertyValue(SeedProperty)!; set => SetPropertyValue(SeedProperty, value); }
    public D2D1_TURBULENCE_NOISE Noise { get => (D2D1_TURBULENCE_NOISE)GetPropertyValue(NoiseProperty)!; set => SetPropertyValue(NoiseProperty, value); }
    public bool Stitchable { get => (bool)GetPropertyValue(StitchableProperty)!; set => SetPropertyValue(StitchableProperty, value); }
}
