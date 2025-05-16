﻿namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1TemperatureTintString)]
#else
[Guid(Constants.CLSID_D2D1TemperatureTintString)]
#endif
public partial class TemperatureTintEffect : EffectWithSource
{
    public static EffectProperty TemperatureProperty { get; }
    public static EffectProperty TintProperty { get; }

    static TemperatureTintEffect()
    {
        TemperatureProperty = EffectProperty.Add(typeof(TemperatureTintEffect), nameof(Temperature), 0, 0f);
        TintProperty = EffectProperty.Add(typeof(TemperatureTintEffect), nameof(Tint), 1, 0f);
    }

    public float Temperature { get => (float)GetPropertyValue(TemperatureProperty)!; set => SetPropertyValue(TemperatureProperty, value.Clamp(-1f, 1f)); }
    public float Tint { get => (float)GetPropertyValue(TintProperty)!; set => SetPropertyValue(TintProperty, value.Clamp(-1f, 1f)); }
}
