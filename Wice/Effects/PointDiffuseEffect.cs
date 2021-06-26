using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1PointDiffuseString)]
    public class PointDiffuseEffect : EffectWithSource
    {
        public static EffectProperty LightPositionProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(LightPosition), 0, new D2D_VECTOR_3F());
        public static EffectProperty DiffuseConstantProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(DiffuseConstant), 1, 1f);
        public static EffectProperty SurfaceScaleProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(SurfaceScale), 2, 1f);
        public static EffectProperty ColorProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(Color), 3, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3, new D2D_VECTOR_3F(1f, 1f, 1f));
        public static EffectProperty KernelUnitLengthProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(KernelUnitLength), 4, new D2D_VECTOR_2F(1f, 1f));
        public static EffectProperty ScaleModeProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(ScaleMode), 5, D2D1_POINTDIFFUSE_SCALE_MODE.D2D1_POINTDIFFUSE_SCALE_MODE_LINEAR);

        public D2D_VECTOR_3F LightPosition { get => (D2D_VECTOR_3F)GetPropertyValue(LightPositionProperty); set => SetPropertyValue(LightPositionProperty, value); }
        public float DiffuseConstant { get => (float)GetPropertyValue(DiffuseConstantProperty); set => SetPropertyValue(DiffuseConstantProperty, value.Clamp(0f, 10000f)); }
        public float SurfaceScale { get => (float)GetPropertyValue(SurfaceScaleProperty); set => SetPropertyValue(SurfaceScaleProperty, value.Clamp(0f, 10000f)); }
        public D2D_VECTOR_3F Color { get => (D2D_VECTOR_3F)GetPropertyValue(ColorProperty); set => SetPropertyValue(ColorProperty, value); }
        public D2D_VECTOR_2F KernelUnitLength { get => (D2D_VECTOR_2F)GetPropertyValue(KernelUnitLengthProperty); set => SetPropertyValue(KernelUnitLengthProperty, value); }
        public D2D1_POINTDIFFUSE_SCALE_MODE ScaleMode { get => (D2D1_POINTDIFFUSE_SCALE_MODE)GetPropertyValue(ScaleModeProperty); set => SetPropertyValue(ScaleModeProperty, value); }
    }
}
