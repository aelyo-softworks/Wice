using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1SaturationString)]
    public class SaturationEffect : EffectWithSource
    {
        public static EffectProperty SaturationProperty = EffectProperty.Add(typeof(SaturationEffect), nameof(Saturation), 0, 0.5f);

        // microsoft-ui-xaml\dev\Effects\microsoft.ui.composition.effects_impl.h defines values between 0 and 2 (doc says 0 and 1)
        public float Saturation { get => (float)GetPropertyValue(SaturationProperty); set => SetPropertyValue(SaturationProperty, value.Clamp(0f, 2f)); }
    }
}
