namespace Wice.Effects;

/// <summary>
/// Represents an effect that simulates the appearance of a distant specular light source illuminating a surface.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1DistantSpecularString)]
#else
[Guid(Constants.CLSID_D2D1DistantSpecularString)]
#endif
public partial class DistantSpecularEffect : EffectWithSource
{
    /// <summary>
    /// Gets the effect property that represents the azimuth angle of the sound source.
    /// </summary>
    public static EffectProperty AzimuthProperty { get; }

    /// <summary>
    /// Gets the property that represents the elevation effect applied to an element.
    /// </summary>
    public static EffectProperty ElevationProperty { get; }

    /// <summary>
    /// Gets the effect property that represents the specular exponent used in lighting calculations.   
    /// </summary>
    public static EffectProperty SpecularExponentProperty { get; }

    /// <summary>
    /// Gets the effect property that represents the specular constant used in lighting calculations.
    /// </summary>
    public static EffectProperty SpecularConstantProperty { get; }

    /// <summary>
    /// Gets the dependency property that identifies the surface scale effect parameter.
    /// </summary>
    public static EffectProperty SurfaceScaleProperty { get; }

    /// <summary>
    /// Gets the effect property that represents the color configuration for the effect.
    /// </summary>
    public static EffectProperty ColorProperty { get; }

    /// <summary>
    /// Gets the effect property that specifies the size of the kernel unit length used in the effect.  
    /// </summary>
    public static EffectProperty KernelUnitLengthProperty { get; }

    /// <summary>
    /// Gets the dependency property that specifies the scaling mode for the effect.
    /// </summary>
    public static EffectProperty ScaleModeProperty { get; }

    static DistantSpecularEffect()
    {
        AzimuthProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(Azimuth), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
        ElevationProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(Elevation), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
        SpecularExponentProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(SpecularExponent), 2, 1f);
        SpecularConstantProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(SpecularConstant), 3, 1f);
        SurfaceScaleProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(SurfaceScale), 4, 1f);
        ColorProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(Color), 5, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3, new D2D_VECTOR_3F(1f, 1f, 1f));
        KernelUnitLengthProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(KernelUnitLength), 6, new D2D_VECTOR_2F(1f, 1f));
        ScaleModeProperty = EffectProperty.Add(typeof(DistantSpecularEffect), nameof(ScaleMode), 7, D2D1_DISTANTDIFFUSE_SCALE_MODE.D2D1_DISTANTDIFFUSE_SCALE_MODE_LINEAR);
    }

    /// <summary>
    /// Gets or sets the azimuth angle, in degrees, representing the horizontal direction of an object.
    /// </summary>
    public float Azimuth { get => (float)GetPropertyValue(AzimuthProperty)!; set => SetPropertyValue(AzimuthProperty, value.Clamp(0f, 360f)); }

    /// <summary>
    /// Gets or sets the elevation angle in degrees.
    /// </summary>
    public float Elevation { get => (float)GetPropertyValue(ElevationProperty)!; set => SetPropertyValue(ElevationProperty, value.Clamp(0f, 360f)); }

    /// <summary>
    /// Gets or sets the specular exponent, which controls the shininess of the surface in lighting calculations.
    /// </summary>
    public float SpecularExponent { get => (float)GetPropertyValue(SpecularExponentProperty)!; set => SetPropertyValue(SpecularExponentProperty, value.Clamp(1f, 128f)); }

    /// <summary>
    /// Gets or sets the specular constant, which determines the intensity of specular highlights in the material.  
    /// </summary>
    public float SpecularConstant { get => (float)GetPropertyValue(SpecularConstantProperty)!; set => SetPropertyValue(SpecularConstantProperty, value.Clamp(0f, 10000f)); }

    /// <summary>
    /// Gets or sets the scale factor applied to the surface geometry of the visual element.    
    /// </summary>
    public float SurfaceScale { get => (float)GetPropertyValue(SurfaceScaleProperty)!; set => SetPropertyValue(SurfaceScaleProperty, value.Clamp(0f, 10000f)); }

    /// <summary>
    /// Gets or sets the color value represented as a 3D vector.
    /// </summary>
    public D2D_VECTOR_3F Color { get => (D2D_VECTOR_3F)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }

    /// <summary>
    /// Gets or sets the kernel unit length for the effect.
    /// </summary>
    public D2D_VECTOR_2F KernelUnitLength { get => (D2D_VECTOR_2F)GetPropertyValue(KernelUnitLengthProperty)!; set => SetPropertyValue(KernelUnitLengthProperty, value); }

    /// <summary>
    /// Gets or sets the scaling mode used for rendering distant diffuse effects.
    /// </summary>
    public D2D1_DISTANTDIFFUSE_SCALE_MODE ScaleMode { get => (D2D1_DISTANTDIFFUSE_SCALE_MODE)GetPropertyValue(ScaleModeProperty)!; set => SetPropertyValue(ScaleModeProperty, value); }
}
