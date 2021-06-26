using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1SharpenString)]
    public class SharpenEffect : EffectWithSource
    {
        public static EffectProperty SharpnessProperty = EffectProperty.Add(typeof(SharpenEffect), nameof(Sharpness), 0, 0f);
        public static EffectProperty ThresholdProperty = EffectProperty.Add(typeof(SharpenEffect), nameof(Threshold), 1, 0f);

        public float Sharpness { get => (float)GetPropertyValue(SharpnessProperty); set => SetPropertyValue(SharpnessProperty, value.Clamp(0f, 10f)); }
        public float Threshold { get => (float)GetPropertyValue(ThresholdProperty); set => SetPropertyValue(ThresholdProperty, value.Clamp(0f, 1f)); }
    }
}
