using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1BrightnessString)]
    public class BrightnessEffect : EffectWithSource
    {
        public static EffectProperty WhitePointProperty = EffectProperty.Add(typeof(BrightnessEffect), nameof(WhitePoint), 0, new D2D_VECTOR_2F(1f, 1f));
        public static EffectProperty BlackPointProperty = EffectProperty.Add(typeof(BrightnessEffect), nameof(BlackPoint), 1, new D2D_VECTOR_2F());

        public D2D_VECTOR_2F WhitePoint { get => (D2D_VECTOR_2F)GetPropertyValue(WhitePointProperty); set => SetPropertyValue(WhitePointProperty, value); }
        public D2D_VECTOR_2F BlackPoint { get => (D2D_VECTOR_2F)GetPropertyValue(BlackPointProperty); set => SetPropertyValue(BlackPointProperty, value); }
    }
}
