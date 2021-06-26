using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1ScaleString)]
    public class ScaleEffect : EffectWithSource
    {
        public static EffectProperty ScaleProperty = EffectProperty.Add(typeof(ScaleEffect), nameof(Scale), 0, new D2D_VECTOR_2F(1f, 1f));
        public static EffectProperty CenterPointProperty = EffectProperty.Add(typeof(ScaleEffect), nameof(CenterPoint), 1, new D2D_VECTOR_2F(0f, 0f));
        public static EffectProperty InterpolationModeProperty = EffectProperty.Add(typeof(ScaleEffect), nameof(InterpolationMode), 2, D2D1_SCALE_INTERPOLATION_MODE.D2D1_SCALE_INTERPOLATION_MODE_LINEAR);
        public static EffectProperty BorderModeProperty = EffectProperty.Add(typeof(ScaleEffect), nameof(BorderMode), 3, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
        public static EffectProperty SharpnessProperty = EffectProperty.Add(typeof(ScaleEffect), nameof(Sharpness), 4, 0f);

        public D2D_VECTOR_2F Scale { get => (D2D_VECTOR_2F)GetPropertyValue(ScaleProperty); set => SetPropertyValue(ScaleProperty, value); }
        public D2D_VECTOR_2F CenterPoint { get => (D2D_VECTOR_2F)GetPropertyValue(CenterPointProperty); set => SetPropertyValue(CenterPointProperty, value); }
        public D2D1_SCALE_INTERPOLATION_MODE InterpolationMode { get => (D2D1_SCALE_INTERPOLATION_MODE)GetPropertyValue(InterpolationModeProperty); set => SetPropertyValue(InterpolationModeProperty, value); }
        public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty); set => SetPropertyValue(BorderModeProperty, value); }
        public float Sharpness { get => (float)GetPropertyValue(SharpnessProperty); set => SetPropertyValue(SharpnessProperty, value.Clamp(0f, 1f)); }
    }
}
