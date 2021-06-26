using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1HdrToneMapString)]
    public class HdrToneMapEffect : EffectWithSource
    {
        public static EffectProperty InputMaxLuminanceProperty = EffectProperty.Add(typeof(HdrToneMapEffect), nameof(InputMaxLuminance), 0, 0f);
        public static EffectProperty OutputMaxLuminanceProperty = EffectProperty.Add(typeof(HdrToneMapEffect), nameof(OutputMaxLuminance), 1, 0f);
        public static EffectProperty DisplayModeProperty = EffectProperty.Add(typeof(HdrToneMapEffect), nameof(DisplayMode), 2, D2D1_HDRTONEMAP_DISPLAY_MODE.D2D1_HDRTONEMAP_DISPLAY_MODE_SDR);

        public float InputMaxLuminance { get => (float)GetPropertyValue(InputMaxLuminanceProperty); set => SetPropertyValue(InputMaxLuminanceProperty, value); }
        public float OutputMaxLuminance { get => (float)GetPropertyValue(OutputMaxLuminanceProperty); set => SetPropertyValue(OutputMaxLuminanceProperty, value); }
        public D2D1_HDRTONEMAP_DISPLAY_MODE DisplayMode { get => (D2D1_HDRTONEMAP_DISPLAY_MODE)GetPropertyValue(DisplayModeProperty); set => SetPropertyValue(DisplayModeProperty, value); }
    }
}
