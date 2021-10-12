using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1DirectionalBlurString)]
    public class DirectionalBlurEffect : EffectWithSource
    {
        public static EffectProperty StandardDeviationProperty { get; }
        public static EffectProperty AngleProperty { get; }
        public static EffectProperty OptimizationProperty { get; }
        public static EffectProperty BorderModeProperty { get; }

        static DirectionalBlurEffect()
        {
            StandardDeviationProperty = EffectProperty.Add(typeof(DirectionalBlurEffect), nameof(StandardDeviation), 0, 3f);
            AngleProperty = EffectProperty.Add(typeof(DirectionalBlurEffect), nameof(Angle), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
            OptimizationProperty = EffectProperty.Add(typeof(DirectionalBlurEffect), nameof(Optimization), 2, D2D1_DIRECTIONALBLUR_OPTIMIZATION.D2D1_DIRECTIONALBLUR_OPTIMIZATION_BALANCED);
            BorderModeProperty = EffectProperty.Add(typeof(DirectionalBlurEffect), nameof(BorderMode), 3, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
        }

        public float StandardDeviation { get => (float)GetPropertyValue(StandardDeviationProperty); set => SetPropertyValue(StandardDeviationProperty, value); }
        public float Angle { get => (float)GetPropertyValue(AngleProperty); set => SetPropertyValue(AngleProperty, value); }
        public D2D1_DIRECTIONALBLUR_OPTIMIZATION Optimization { get => (D2D1_DIRECTIONALBLUR_OPTIMIZATION)GetPropertyValue(OptimizationProperty); set => SetPropertyValue(OptimizationProperty, value); }
        public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty); set => SetPropertyValue(BorderModeProperty, value); }
    }
}
