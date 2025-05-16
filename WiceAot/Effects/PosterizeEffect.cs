namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1PosterizeString)]
#else
[Guid(Constants.CLSID_D2D1PosterizeString)]
#endif
public partial class PosterizeEffect : EffectWithSource
{
    public static EffectProperty RedValueCountProperty { get; }
    public static EffectProperty GreeValueCountProperty { get; }
    public static EffectProperty BlueValueCountProperty { get; }

    static PosterizeEffect()
    {
        RedValueCountProperty = EffectProperty.Add(typeof(PosterizeEffect), nameof(RedValueCount), 0, 4);
        GreeValueCountProperty = EffectProperty.Add(typeof(PosterizeEffect), nameof(GreeValueCount), 1, 4);
        BlueValueCountProperty = EffectProperty.Add(typeof(PosterizeEffect), nameof(BlueValueCount), 2, 4);
    }

    public int RedValueCount { get => (int)GetPropertyValue(RedValueCountProperty)!; set => SetPropertyValue(RedValueCountProperty, value.Clamp(2, 16)); }
    public int GreeValueCount { get => (int)GetPropertyValue(GreeValueCountProperty)!; set => SetPropertyValue(GreeValueCountProperty, value.Clamp(2, 16)); }
    public int BlueValueCount { get => (int)GetPropertyValue(BlueValueCountProperty)!; set => SetPropertyValue(BlueValueCountProperty, value.Clamp(2, 16)); }
}
