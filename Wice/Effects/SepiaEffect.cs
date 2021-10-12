using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1SepiaString)]
    public class SepiaEffect : EffectWithSource
    {
        public static EffectProperty IntensityProperty { get; }
        public static EffectProperty AlphaModeProperty { get; }

        static SepiaEffect()
        {
            IntensityProperty = EffectProperty.Add(typeof(SepiaEffect), nameof(Intensity), 0, 0.5f);
            AlphaModeProperty = EffectProperty.Add(typeof(SepiaEffect), nameof(AlphaMode), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLORMATRIX_ALPHA_MODE, D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED);
        }

        public float Intensity { get => (float)GetPropertyValue(IntensityProperty); set => SetPropertyValue(IntensityProperty, value.Clamp(0f, 1f)); }
        public D2D1_ALPHA_MODE AlphaMode { get => (D2D1_ALPHA_MODE)GetPropertyValue(AlphaModeProperty); set => SetPropertyValue(AlphaModeProperty, value); }
    }
}
