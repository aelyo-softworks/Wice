using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1ExposureString)]
    public class ExposureEffect : EffectWithSource
    {
        public static EffectProperty ExposureValueProperty { get; }

        static ExposureEffect()
        {
            ExposureValueProperty = EffectProperty.Add(typeof(ExposureEffect), nameof(ExposureValue), 0, 0f);
        }

        public float ExposureValue { get => (float)GetPropertyValue(ExposureValueProperty); set => SetPropertyValue(ExposureValueProperty, value.Clamp(-2f, 2f)); }
    }
}
