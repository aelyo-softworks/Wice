using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1ShadowString)]
    public class ShadowEffect : EffectWithSource
    {
        public static EffectProperty BlurStandardDeviationProperty { get; }
        public static EffectProperty ColorProperty { get; }
        public static EffectProperty OptimizationProperty { get; }

        static ShadowEffect()
        {
            BlurStandardDeviationProperty = EffectProperty.Add(typeof(ShadowEffect), nameof(BlurStandardDeviation), 0, 3f);
            ColorProperty = EffectProperty.Add(typeof(ShadowEffect), nameof(Color), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR4, new _D3DCOLORVALUE(1, 0, 0, 0));
            OptimizationProperty = EffectProperty.Add(typeof(ShadowEffect), nameof(Optimization), 0, D2D1_SHADOW_OPTIMIZATION.D2D1_SHADOW_OPTIMIZATION_BALANCED);
        }

        public float BlurStandardDeviation { get => (float)GetPropertyValue(BlurStandardDeviationProperty); set => SetPropertyValue(BlurStandardDeviationProperty, value); }
        public _D3DCOLORVALUE Color { get => (_D3DCOLORVALUE)GetPropertyValue(ColorProperty); set => SetPropertyValue(ColorProperty, value); }
        public D2D1_SHADOW_OPTIMIZATION Optimization { get => (D2D1_SHADOW_OPTIMIZATION)GetPropertyValue(OptimizationProperty); set => SetPropertyValue(OptimizationProperty, value); }
    }
}
