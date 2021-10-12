using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1HueToRgbString)]
    public class HueToRgbEffect : EffectWithSource
    {
        public static EffectProperty InputColorSpaceProperty { get; }

        static HueToRgbEffect()
        {
            InputColorSpaceProperty = EffectProperty.Add(typeof(HueToRgbEffect), nameof(InputColorSpace), 0, D2D1_HUETORGB_INPUT_COLOR_SPACE.D2D1_HUETORGB_INPUT_COLOR_SPACE_HUE_SATURATION_VALUE);
        }

        public D2D1_HUETORGB_INPUT_COLOR_SPACE InputColorSpace { get => (D2D1_HUETORGB_INPUT_COLOR_SPACE)GetPropertyValue(InputColorSpaceProperty); set => SetPropertyValue(InputColorSpaceProperty, value); }
    }
}
