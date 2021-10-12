using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1DiscreteTransferString)]
    public class DiscreteTransferEffect : EffectWithSource
    {
        public static EffectProperty RedTableProperty { get; }
        public static EffectProperty RedDisableProperty { get; }
        public static EffectProperty GreenTableProperty { get; }
        public static EffectProperty GreenDisableProperty { get; }
        public static EffectProperty BlueTableProperty { get; }
        public static EffectProperty BlueDisableProperty { get; }
        public static EffectProperty AlphaTableProperty { get; }
        public static EffectProperty AlphaDisableProperty { get; }
        public static EffectProperty ClampOutputProperty { get; }

        static DiscreteTransferEffect()
        {
            RedTableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(RedTable), 0, new float[] { 0f, 1f });
            RedDisableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(RedDisable), 1, false);
            GreenTableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(GreenTable), 2, new float[] { 0f, 1f });
            GreenDisableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(GreenDisable), 3, false);
            BlueTableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(BlueTable), 4, new float[] { 0f, 1f });
            BlueDisableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(BlueDisable), 5, false);
            AlphaTableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(AlphaTable), 6, new float[] { 0f, 1f });
            AlphaDisableProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(AlphaDisable), 7, false);
            ClampOutputProperty = EffectProperty.Add(typeof(DiscreteTransferEffect), nameof(ClampOutput), 8, false);
        }

        public float[] RedTable { get => (float[])GetPropertyValue(RedTableProperty); set => SetPropertyValue(RedTableProperty, value); }
        public bool RedDisable { get => (bool)GetPropertyValue(RedDisableProperty); set => SetPropertyValue(RedDisableProperty, value); }
        public float[] GreenTable { get => (float[])GetPropertyValue(GreenTableProperty); set => SetPropertyValue(GreenTableProperty, value); }
        public bool GreenDisable { get => (bool)GetPropertyValue(GreenDisableProperty); set => SetPropertyValue(GreenDisableProperty, value); }
        public float[] BlueTable { get => (float[])GetPropertyValue(BlueTableProperty); set => SetPropertyValue(BlueTableProperty, value); }
        public bool BlueDisable { get => (bool)GetPropertyValue(BlueDisableProperty); set => SetPropertyValue(BlueDisableProperty, value); }
        public float[] AlphaTable { get => (float[])GetPropertyValue(AlphaTableProperty); set => SetPropertyValue(AlphaTableProperty, value); }
        public bool AlphaDisable { get => (bool)GetPropertyValue(AlphaDisableProperty); set => SetPropertyValue(AlphaDisableProperty, value); }
        public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty); set => SetPropertyValue(ClampOutputProperty, value); }
    }
}
