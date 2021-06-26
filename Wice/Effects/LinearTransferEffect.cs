using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1LinearTransferString)]
    public class LinearTransferEffect : EffectWithSource
    {
        public static EffectProperty RedYInterceptProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(RedYIntercept), 0, 0f);
        public static EffectProperty RedSlopeProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(RedSlope), 1, 1f);
        public static EffectProperty RedDisableProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(RedDisable), 2, false);
        public static EffectProperty GreenYInterceptProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(GreenYIntercept), 3, 0f);
        public static EffectProperty GreenSlopeProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(GreenSlope), 4, 1f);
        public static EffectProperty GreenDisableProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(GreenDisable), 5, false);
        public static EffectProperty BlueYInterceptProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(BlueYIntercept), 6, 0f);
        public static EffectProperty BlueSlopeProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(BlueSlope), 7, 1f);
        public static EffectProperty BlueDisableProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(BlueDisable), 8, false);
        public static EffectProperty AlphaYInterceptProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(AlphaYIntercept), 9, 0f);
        public static EffectProperty AlphaSlopeProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(AlphaSlope), 10, 1f);
        public static EffectProperty AlphaDisableProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(AlphaDisable), 11, false);
        public static EffectProperty ClampOutputProperty = EffectProperty.Add(typeof(LinearTransferEffect), nameof(ClampOutput), 12, false);

        public float RedYIntercept { get => (float)GetPropertyValue(RedYInterceptProperty); set => SetPropertyValue(RedYInterceptProperty, value); }
        public float RedSlope { get => (float)GetPropertyValue(RedSlopeProperty); set => SetPropertyValue(RedSlopeProperty, value); }
        public bool RedDisable { get => (bool)GetPropertyValue(RedDisableProperty); set => SetPropertyValue(RedDisableProperty, value); }
        public float GreenYIntercept { get => (float)GetPropertyValue(GreenYInterceptProperty); set => SetPropertyValue(GreenYInterceptProperty, value); }
        public float GreenSlope { get => (float)GetPropertyValue(GreenSlopeProperty); set => SetPropertyValue(GreenSlopeProperty, value); }
        public bool GreenDisable { get => (bool)GetPropertyValue(GreenDisableProperty); set => SetPropertyValue(GreenDisableProperty, value); }
        public float BlueYIntercept { get => (float)GetPropertyValue(BlueYInterceptProperty); set => SetPropertyValue(BlueYInterceptProperty, value); }
        public float BlueSlope { get => (float)GetPropertyValue(BlueSlopeProperty); set => SetPropertyValue(BlueSlopeProperty, value); }
        public bool BlueDisable { get => (bool)GetPropertyValue(BlueDisableProperty); set => SetPropertyValue(BlueDisableProperty, value); }
        public float AlphaYIntercept { get => (float)GetPropertyValue(AlphaYInterceptProperty); set => SetPropertyValue(AlphaYInterceptProperty, value); }
        public float AlphaSlope { get => (float)GetPropertyValue(AlphaSlopeProperty); set => SetPropertyValue(AlphaSlopeProperty, value); }
        public bool AlphaDisable { get => (bool)GetPropertyValue(AlphaDisableProperty); set => SetPropertyValue(AlphaDisableProperty, value); }
        public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty); set => SetPropertyValue(ClampOutputProperty, value); }
    }
}
