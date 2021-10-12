using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1ContrastString)]
    public class ContrastEffect : EffectWithSource
    {
        public static EffectProperty ContrastProperty { get; }
        public static EffectProperty ClampInputProperty { get; }

        static ContrastEffect()
        {
            ContrastProperty = EffectProperty.Add(typeof(ContrastEffect), nameof(Contrast), 0, 0f);
            ClampInputProperty = EffectProperty.Add(typeof(ContrastEffect), nameof(ClampInput), 1, false);
        }

        public float Contrast { get => (float)GetPropertyValue(ContrastProperty); set => SetPropertyValue(ContrastProperty, value.Clamp(-1f, 1f)); }
        public bool ClampInput { get => (bool)GetPropertyValue(ClampInputProperty); set => SetPropertyValue(ClampInputProperty, value); }
    }
}
