using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1PosterizeString)]
    public class PosterizeEffect : EffectWithSource
    {
        public static EffectProperty RedValueCountProperty = EffectProperty.Add(typeof(PosterizeEffect), nameof(RedValueCount), 0, 4);
        public static EffectProperty GreeValueCountProperty = EffectProperty.Add(typeof(PosterizeEffect), nameof(GreeValueCount), 1, 4);
        public static EffectProperty BlueValueCountProperty = EffectProperty.Add(typeof(PosterizeEffect), nameof(BlueValueCount), 2, 4);

        public int RedValueCount { get => (int)GetPropertyValue(RedValueCountProperty); set => SetPropertyValue(RedValueCountProperty, value.Clamp(2, 16)); }
        public int GreeValueCount { get => (int)GetPropertyValue(GreeValueCountProperty); set => SetPropertyValue(GreeValueCountProperty, value.Clamp(2, 16)); }
        public int BlueValueCount { get => (int)GetPropertyValue(BlueValueCountProperty); set => SetPropertyValue(BlueValueCountProperty, value.Clamp(2, 16)); }
    }
}
