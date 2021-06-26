using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1TableTransferString)]
    public class TableTransferEffect : EffectWithSource
    {
        public static EffectProperty RedTableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(RedTable), 0, new float[] { 0f, 1f });
        public static EffectProperty RedDisableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(RedDisable), 1, false);
        public static EffectProperty GreenTableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(GreenTable), 2, new float[] { 0f, 1f });
        public static EffectProperty GreenDisableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(GreenDisable), 3, false);
        public static EffectProperty BlueTableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(BlueTable), 4, new float[] { 0f, 1f });
        public static EffectProperty BlueDisableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(BlueDisable), 5, false);
        public static EffectProperty AlphaTableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(AlphaTable), 6, new float[] { 0f, 1f });
        public static EffectProperty AlphaDisableProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(AlphaDisable), 7, false);
        public static EffectProperty ClampOutputProperty = EffectProperty.Add(typeof(TableTransferEffect), nameof(ClampOutput), 8, false);

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
