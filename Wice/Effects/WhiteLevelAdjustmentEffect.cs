using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1WhiteLevelAdjustmentString)]
    public class WhiteLevelAdjustmentEffect : EffectWithSource
    {
        public static EffectProperty InputWhiteLevelProperty { get; }
        public static EffectProperty OutputWhiteLevelProperty { get; }

        static WhiteLevelAdjustmentEffect()
        {
            InputWhiteLevelProperty = EffectProperty.Add(typeof(WhiteLevelAdjustmentEffect), nameof(InputWhiteLevel), 0, 0f);
            OutputWhiteLevelProperty = EffectProperty.Add(typeof(WhiteLevelAdjustmentEffect), nameof(OutputWhiteLevel), 1, 0f);
        }

        public float InputWhiteLevel { get => (float)GetPropertyValue(InputWhiteLevelProperty); set => SetPropertyValue(InputWhiteLevelProperty, value); }
        public float OutputWhiteLevel { get => (float)GetPropertyValue(OutputWhiteLevelProperty); set => SetPropertyValue(OutputWhiteLevelProperty, value); }
    }
}
