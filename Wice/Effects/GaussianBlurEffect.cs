using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1GaussianBlurString)]
    public class GaussianBlurEffect : EffectWithSource
    {
        public static EffectProperty StandardDeviationProperty = EffectProperty.Add(typeof(GaussianBlurEffect), nameof(StandardDeviation), 0, 3f);
        public static EffectProperty OptimizationProperty = EffectProperty.Add(typeof(GaussianBlurEffect), nameof(Optimization), 1, D2D1_GAUSSIANBLUR_OPTIMIZATION.D2D1_GAUSSIANBLUR_OPTIMIZATION_BALANCED);
        public static EffectProperty BorderModeProperty = EffectProperty.Add(typeof(GaussianBlurEffect), nameof(BorderMode), 2, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);

        public float StandardDeviation { get => (float)GetPropertyValue(StandardDeviationProperty); set => SetPropertyValue(StandardDeviationProperty, value); }
        public D2D1_GAUSSIANBLUR_OPTIMIZATION Optimization { get => (D2D1_GAUSSIANBLUR_OPTIMIZATION)GetPropertyValue(OptimizationProperty); set => SetPropertyValue(OptimizationProperty, value); }
        public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty); set => SetPropertyValue(BorderModeProperty, value); }
    }
}
