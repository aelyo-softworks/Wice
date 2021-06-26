using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1DistantSpecularString)]
    public class DistantSpecularEffect : EffectWithSource
    {
        public static EffectProperty AzimuthProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(Azimuth), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
        public static EffectProperty ElevationProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(Elevation), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
        public static EffectProperty SpecularExponentProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(SpecularExponent), 2, 1f);
        public static EffectProperty SpecularConstantProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(SpecularConstant), 3, 1f);
        public static EffectProperty SurfaceScaleProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(SurfaceScale), 4, 1f);
        public static EffectProperty ColorProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(Color), 5, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3, new D2D_VECTOR_3F(1f, 1f, 1f));
        public static EffectProperty KernelUnitLengthProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(KernelUnitLength), 6, new D2D_VECTOR_2F(1f, 1f));
        public static EffectProperty ScaleModeProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(ScaleMode), 7, D2D1_DISTANTDIFFUSE_SCALE_MODE.D2D1_DISTANTDIFFUSE_SCALE_MODE_LINEAR);

        public float Azimuth { get => (float)GetPropertyValue(AzimuthProperty); set => SetPropertyValue(AzimuthProperty, value.Clamp(0f, 360f)); }
        public float Elevation { get => (float)GetPropertyValue(ElevationProperty); set => SetPropertyValue(ElevationProperty, value.Clamp(0f, 360f)); }
        public float SpecularExponent { get => (float)GetPropertyValue(SpecularExponentProperty); set => SetPropertyValue(SpecularExponentProperty, value.Clamp(1f, 128f)); }
        public float SpecularConstant { get => (float)GetPropertyValue(SpecularConstantProperty); set => SetPropertyValue(SpecularConstantProperty, value.Clamp(0f, 10000f)); }
        public float SurfaceScale { get => (float)GetPropertyValue(SurfaceScaleProperty); set => SetPropertyValue(SurfaceScaleProperty, value.Clamp(0f, 10000f)); }
        public D2D_VECTOR_3F Color { get => (D2D_VECTOR_3F)GetPropertyValue(ColorProperty); set => SetPropertyValue(ColorProperty, value); }
        public D2D_VECTOR_2F KernelUnitLength { get => (D2D_VECTOR_2F)GetPropertyValue(KernelUnitLengthProperty); set => SetPropertyValue(KernelUnitLengthProperty, value); }
        public D2D1_DISTANTDIFFUSE_SCALE_MODE ScaleMode { get => (D2D1_DISTANTDIFFUSE_SCALE_MODE)GetPropertyValue(ScaleModeProperty); set => SetPropertyValue(ScaleModeProperty, value); }
    }
}
