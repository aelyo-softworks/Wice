using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D13DTransformString)]
    public class TransformEffect : EffectWithSource
    {
        public static EffectProperty InterpolationModeProperty { get; }
        public static EffectProperty BorderModeProperty { get; }
        public static EffectProperty TransformMatrixProperty { get; }

        static TransformEffect()
        {
            InterpolationModeProperty = EffectProperty.Add(typeof(TransformEffect), nameof(InterpolationMode), 0, D2D1_3DTRANSFORM_INTERPOLATION_MODE.D2D1_3DTRANSFORM_INTERPOLATION_MODE_LINEAR);
            BorderModeProperty = EffectProperty.Add(typeof(TransformEffect), nameof(BorderMode), 1, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
            TransformMatrixProperty = EffectProperty.Add(typeof(TransformEffect), nameof(TransformMatrix), 2, new D2D_MATRIX_4X4_F(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f));
        }

        public D2D1_3DTRANSFORM_INTERPOLATION_MODE InterpolationMode { get => (D2D1_3DTRANSFORM_INTERPOLATION_MODE)GetPropertyValue(InterpolationModeProperty); set => SetPropertyValue(InterpolationModeProperty, value); }
        public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty); set => SetPropertyValue(BorderModeProperty, value); }
        public D2D_MATRIX_4X4_F TransformMatrix { get => (D2D_MATRIX_4X4_F)GetPropertyValue(TransformMatrixProperty); set => SetPropertyValue(TransformMatrixProperty, value); }
    }
}
