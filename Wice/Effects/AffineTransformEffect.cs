using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D12DAffineTransformString)]
    public class AffineTransformEffect : EffectWithSource
    {
        public static EffectProperty InterpolationModeProperty = EffectProperty.Add(typeof(AffineTransformEffect), nameof(InterpolationMode), 0, D2D1_2DAFFINETRANSFORM_INTERPOLATION_MODE.D2D1_2DAFFINETRANSFORM_INTERPOLATION_MODE_LINEAR);
        public static EffectProperty BorderModeProperty = EffectProperty.Add(typeof(AffineTransformEffect), nameof(BorderMode), 1, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
        public static EffectProperty TransformMatrixProperty = EffectProperty.Add(typeof(AffineTransformEffect), nameof(TransformMatrix), 2, D2D_MATRIX_3X2_F.Identity());
        public static EffectProperty SharpnessProperty = EffectProperty.Add(typeof(AffineTransformEffect), nameof(Sharpness), 3, 1f);

        public D2D1_2DAFFINETRANSFORM_INTERPOLATION_MODE InterpolationMode { get => (D2D1_2DAFFINETRANSFORM_INTERPOLATION_MODE)GetPropertyValue(InterpolationModeProperty); set => SetPropertyValue(InterpolationModeProperty, value); }
        public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty); set => SetPropertyValue(BorderModeProperty, value); }
        public D2D_MATRIX_3X2_F TransformMatrix { get => (D2D_MATRIX_3X2_F)GetPropertyValue(TransformMatrixProperty); set => SetPropertyValue(TransformMatrixProperty, value); }
        public float Sharpness { get => (float)GetPropertyValue(SharpnessProperty); set => SetPropertyValue(SharpnessProperty, value.Clamp(0f, 1f)); }
    }
}
