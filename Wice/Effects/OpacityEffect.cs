using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1OpacityString)]
    public class OpacityEffect : EffectWithSource
    {
        public static EffectProperty OpacityProperty = EffectProperty.Add(typeof(OpacityEffect), nameof(Opacity), 0, 1f);

        public float Opacity { get => (float)GetPropertyValue(OpacityProperty); set => SetPropertyValue(OpacityProperty, value); }
    }
}
