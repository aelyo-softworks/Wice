namespace Wice.Effects;

/// <summary>
/// Represents an effect that simulates the appearance of a distant light source illuminating a surface, producing a
/// diffuse reflection based on the surface's properties.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1DistantDiffuseString)]
#else
[Guid(Constants.CLSID_D2D1DistantDiffuseString)]
#endif
public partial class DistantDiffuseEffect : EffectWithSource
{
    /// <summary>
    /// Gets the effect property that represents the azimuth angle of the sound source.
    /// </summary>
    public static EffectProperty AzimuthProperty { get; }

    /// <summary>
    /// Gets the dependency property that represents the elevation effect of a UI element.
    /// </summary>
    public static EffectProperty ElevationProperty { get; }

    /// <summary>
    /// Gets the effect property representing the diffuse constant used in lighting calculations.
    /// </summary>
    public static EffectProperty DiffuseConstantProperty { get; }

    /// <summary>
    /// Gets the dependency property that identifies the surface scale effect parameter.
    /// </summary>
    public static EffectProperty SurfaceScaleProperty { get; }

    /// <summary>
    /// Gets the effect property that represents the color configuration for the effect.
    /// </summary>
    public static EffectProperty ColorProperty { get; }

    /// <summary>
    /// Gets the effect property that specifies the kernel unit length for the effect.
    /// </summary>
    public static EffectProperty KernelUnitLengthProperty { get; }

    /// <summary>
    /// Gets the dependency property that represents the scale mode of the effect.
    /// </summary>
    public static EffectProperty ScaleModeProperty { get; }

    static DistantDiffuseEffect()
    {
        AzimuthProperty = EffectProperty.Add(typeof(DistantDiffuseEffect), nameof(Azimuth), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
        ElevationProperty = EffectProperty.Add(typeof(DistantDiffuseEffect), nameof(Elevation), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 0f);
        DiffuseConstantProperty = EffectProperty.Add(typeof(DistantDiffuseEffect), nameof(DiffuseConstant), 2, 1f);
        SurfaceScaleProperty = EffectProperty.Add(typeof(DistantDiffuseEffect), nameof(SurfaceScale), 3, 1f);
        ColorProperty = EffectProperty.Add(typeof(DistantDiffuseEffect), nameof(Color), 4, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3, new D2D_VECTOR_3F(1f, 1f, 1f));
        KernelUnitLengthProperty = EffectProperty.Add(typeof(DistantDiffuseEffect), nameof(KernelUnitLength), 5, new D2D_VECTOR_2F(1f, 1f));
        ScaleModeProperty = EffectProperty.Add(typeof(DistantDiffuseEffect), nameof(ScaleMode), 6, D2D1_DISTANTDIFFUSE_SCALE_MODE.D2D1_DISTANTDIFFUSE_SCALE_MODE_LINEAR);
    }

    /// <summary>
    /// Gets or sets the azimuth angle, measured in degrees.
    /// </summary>
    public float Azimuth { get => (float)GetPropertyValue(AzimuthProperty)!; set => SetPropertyValue(AzimuthProperty, value.Clamp(0f, 360f)); }

    /// <summary>
    /// Gets or sets the elevation angle in degrees.
    /// </summary>
    public float Elevation { get => (float)GetPropertyValue(ElevationProperty)!; set => SetPropertyValue(ElevationProperty, value.Clamp(0f, 360f)); }

    /// <summary>
    /// Gets or sets the diffuse constant, which determines the intensity of diffuse reflection in the material.
    /// </summary>
    public float DiffuseConstant { get => (float)GetPropertyValue(DiffuseConstantProperty)!; set => SetPropertyValue(DiffuseConstantProperty, value.Clamp(0f, 10000f)); }

    /// <summary>
    /// Gets or sets the scale factor applied to the surface geometry of the object.
    /// </summary>
    public float SurfaceScale { get => (float)GetPropertyValue(SurfaceScaleProperty)!; set => SetPropertyValue(SurfaceScaleProperty, value.Clamp(0f, 10000f)); }

    /// <summary>
    /// Gets or sets the color represented as a three-dimensional vector.
    /// </summary>
    public D2D_VECTOR_3F Color { get => (D2D_VECTOR_3F)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }

    /// <summary>
    /// Gets or sets the kernel unit length for the effect.     
    /// </summary>
    public D2D_VECTOR_2F KernelUnitLength { get => (D2D_VECTOR_2F)GetPropertyValue(KernelUnitLengthProperty)!; set => SetPropertyValue(KernelUnitLengthProperty, value); }

    /// <summary>
    /// Gets or sets the scaling mode used for the distant diffuse effect.
    /// </summary>
    public D2D1_DISTANTDIFFUSE_SCALE_MODE ScaleMode { get => (D2D1_DISTANTDIFFUSE_SCALE_MODE)GetPropertyValue(ScaleModeProperty)!; set => SetPropertyValue(ScaleModeProperty, value); }
}
