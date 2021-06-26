using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1DistantDiffuseString)]
    public class DistantDiffuseEffect : EffectWithSource
    {
        public static EffectProperty AzimuthProperty = EffectProperty.Add(typeof(DistantDiffuseEffect), nameof(Azimuth), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
        public static EffectProperty ElevationProperty = EffectProperty.Add(typeof(DistantDiffuseEffect), nameof(Elevation), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
        public static EffectProperty DiffuseConstantProperty = EffectProperty.Add(typeof(DistantDiffuseEffect), nameof(DiffuseConstant), 2, 1f);
        public static EffectProperty SurfaceScaleProperty = EffectProperty.Add(typeof(DistantDiffuseEffect), nameof(SurfaceScale), 3, 1f);
        public static EffectProperty ColorProperty = EffectProperty.Add(typeof(DistantDiffuseEffect), nameof(Color), 4, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3, new D2D_VECTOR_3F(1f, 1f, 1f));
        public static EffectProperty KernelUnitLengthProperty = EffectProperty.Add(typeof(DistantDiffuseEffect), nameof(KernelUnitLength), 5, new D2D_VECTOR_2F(1f, 1f));
        public static EffectProperty ScaleModeProperty = EffectProperty.Add(typeof(DistantDiffuseEffect), nameof(ScaleMode), 6, D2D1_DISTANTDIFFUSE_SCALE_MODE.D2D1_DISTANTDIFFUSE_SCALE_MODE_LINEAR);

        public float Azimuth { get => (float)GetPropertyValue(AzimuthProperty); set => SetPropertyValue(AzimuthProperty, value.Clamp(0f, 360f)); }
        public float Elevation { get => (float)GetPropertyValue(ElevationProperty); set => SetPropertyValue(ElevationProperty, value.Clamp(0f, 360f)); }
        public float DiffuseConstant { get => (float)GetPropertyValue(DiffuseConstantProperty); set => SetPropertyValue(DiffuseConstantProperty, value.Clamp(0f, 10000f)); }
        public float SurfaceScale { get => (float)GetPropertyValue(SurfaceScaleProperty); set => SetPropertyValue(SurfaceScaleProperty, value.Clamp(0f, 10000f)); }
        public D2D_VECTOR_3F Color { get => (D2D_VECTOR_3F)GetPropertyValue(ColorProperty); set => SetPropertyValue(ColorProperty, value); }
        public D2D_VECTOR_2F KernelUnitLength { get => (D2D_VECTOR_2F)GetPropertyValue(KernelUnitLengthProperty); set => SetPropertyValue(KernelUnitLengthProperty, value); }
        public D2D1_DISTANTDIFFUSE_SCALE_MODE ScaleMode { get => (D2D1_DISTANTDIFFUSE_SCALE_MODE)GetPropertyValue(ScaleModeProperty); set => SetPropertyValue(ScaleModeProperty, value); }
    }
}
