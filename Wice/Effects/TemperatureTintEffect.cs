using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1TemperatureTintString)]
    public class TemperatureTintEffect : EffectWithSource
    {
        public static EffectProperty TemperatureProperty = EffectProperty.Add(typeof(TemperatureTintEffect), nameof(Temperature), 0, 0f);
        public static EffectProperty TintProperty = EffectProperty.Add(typeof(TemperatureTintEffect), nameof(Tint), 1, 0f);

        public float Temperature { get => (float)GetPropertyValue(TemperatureProperty); set => SetPropertyValue(TemperatureProperty, value.Clamp(-1f, 1f)); }
        public float Tint { get => (float)GetPropertyValue(TintProperty); set => SetPropertyValue(TintProperty, value.Clamp(-1f, 1f)); }
    }
}
