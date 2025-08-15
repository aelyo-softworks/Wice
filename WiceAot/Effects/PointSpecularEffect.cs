namespace Wice.Effects;

#if NETFRAMEWORK
/// <summary>
/// CLSID attribute binding this effect to the native D2D PointSpecular implementation on .NET Framework.
/// </summary>
[Guid(D2D1Constants.CLSID_D2D1PointSpecularString)]
#else
/// <summary>
/// CLSID attribute binding this effect to the native D2D PointSpecular implementation on modern .NET.
/// </summary>
[Guid(Constants.CLSID_D2D1PointSpecularString)]
#endif
/// <summary>
/// Wraps the Direct2D PointSpecular lighting effect (D2D1_POINTSPECULAR).
/// </summary>
/// <remarks>
/// - Requires at least one input source (height/normal map) via <see cref="EffectWithSource.Source"/>.
/// - Exposes all D2D1_POINTSPECULAR properties through strongly-typed CLR properties backed by
///   <see cref="EffectProperty"/> descriptors to support D2D interop.
/// - Property indices map to the D2D effect property bag in the same order as the native API:
///   0: LightPosition, 1: SpecularExponent, 2: SpecularConstant, 3: SurfaceScale, 4: Color, 5: KernelUnitLength, 6: ScaleMode.
/// </remarks>
public partial class PointSpecularEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for <see cref="LightPosition"/> (index 0).
    /// </summary>
    public static EffectProperty LightPositionProperty { get; }
    /// <summary>
    /// Descriptor for <see cref="SpecularExponent"/> (index 1).
    /// </summary>
    public static EffectProperty SpecularExponentProperty { get; }
    /// <summary>
    /// Descriptor for <see cref="SpecularConstant"/> (index 2).
    /// </summary>
    public static EffectProperty SpecularConstantProperty { get; }
    /// <summary>
    /// Descriptor for <see cref="SurfaceScale"/> (index 3).
    /// </summary>
    public static EffectProperty SurfaceScaleProperty { get; }
    /// <summary>
    /// Descriptor for <see cref="Color"/> (index 4, mapped as COLOR_TO_VECTOR3).
    /// </summary>
    public static EffectProperty ColorProperty { get; }
    /// <summary>
    /// Descriptor for <see cref="KernelUnitLength"/> (index 5).
    /// </summary>
    public static EffectProperty KernelUnitLengthProperty { get; }
    /// <summary>
    /// Descriptor for <see cref="ScaleMode"/> (index 6).
    /// </summary>
    public static EffectProperty ScaleModeProperty { get; }

    /// <summary>
    /// Registers the effect property descriptors and default values, including D2D mapping semantics.
    /// </summary>
    static PointSpecularEffect()
    {
        // 0: Light position in 3D (x,y in input space units/pixels; z is height/distance).
        LightPositionProperty = EffectProperty.Add(typeof(PointSpecularEffect), nameof(LightPosition), 0, new D2D_VECTOR_3F());
        // 1: Shininess exponent (clamped to [1, 128]).
        SpecularExponentProperty = EffectProperty.Add(typeof(PointSpecularEffect), nameof(SpecularExponent), 1, 1f);
        // 2: Specular intensity scale (clamped to [0, 10000]).
        SpecularConstantProperty = EffectProperty.Add(typeof(PointSpecularEffect), nameof(SpecularConstant), 2, 1f);
        // 3: Surface height scale factor for normal computation (clamped to [0, 10000]).
        SurfaceScaleProperty = EffectProperty.Add(typeof(PointSpecularEffect), nameof(SurfaceScale), 3, 1f);
        // 4: Light color (RGB) mapped from Color to Vector3; default is white (1,1,1).
        ColorProperty = EffectProperty.Add(typeof(PointSpecularEffect), nameof(Color), 4, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3, new D2D_VECTOR_3F(1f, 1f, 1f));
        // 5: Sampling distance in input pixels for gradient computation.
        KernelUnitLengthProperty = EffectProperty.Add(typeof(PointSpecularEffect), nameof(KernelUnitLength), 5, new D2D_VECTOR_2F(1f, 1f));
        // 6: Resampling filter used by the effect (default Linear).
        ScaleModeProperty = EffectProperty.Add(typeof(PointSpecularEffect), nameof(ScaleMode), 6, D2D1_POINTSPECULAR_SCALE_MODE.D2D1_POINTSPECULAR_SCALE_MODE_LINEAR);
    }

    /// <summary>
    /// Gets or sets the position of the point light in 3D space.
    /// </summary>
    /// <remarks>
    /// X/Y are in input space units (typically pixels); Z is the light height/distance above the surface.
    /// Default: (0, 0, 0).
    /// </remarks>
    public D2D_VECTOR_3F LightPosition { get => (D2D_VECTOR_3F)GetPropertyValue(LightPositionProperty)!; set => SetPropertyValue(LightPositionProperty, value); }

    /// <summary>
    /// Gets or sets the specular exponent controlling highlight sharpness (shininess).
    /// </summary>
    /// <remarks>
    /// Valid range: [1, 128]. Values outside the range are clamped.
    /// Default: 1.
    /// </remarks>
    public float SpecularExponent { get => (float)GetPropertyValue(SpecularExponentProperty)!; set => SetPropertyValue(SpecularExponentProperty, value.Clamp(1f, 128f)); }

    /// <summary>
    /// Gets or sets the scalar multiplier for specular highlights (intensity).
    /// </summary>
    /// <remarks>
    /// Valid range: [0, 10000]. Values outside the range are clamped.
    /// Default: 1.
    /// </remarks>
    public float SpecularConstant { get => (float)GetPropertyValue(SpecularConstantProperty)!; set => SetPropertyValue(SpecularConstantProperty, value.Clamp(0f, 10000f)); }

    /// <summary>
    /// Gets or sets the surface height scale used when deriving normals from the input.
    /// </summary>
    /// <remarks>
    /// Larger values increase the perceived surface relief.
    /// Valid range: [0, 10000]. Values outside the range are clamped.
    /// Default: 1.
    /// </remarks>
    public float SurfaceScale { get => (float)GetPropertyValue(SurfaceScaleProperty)!; set => SetPropertyValue(SurfaceScaleProperty, value.Clamp(0f, 10000f)); }

    /// <summary>
    /// Gets or sets the RGB light color as a vector (alpha is not used).
    /// </summary>
    /// <remarks>
    /// Mapped with <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3"/>.
    /// Default: (1, 1, 1) (white).
    /// </remarks>
    public D2D_VECTOR_3F Color { get => (D2D_VECTOR_3F)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }

    /// <summary>
    /// Gets or sets the kernel unit length used to compute gradients in the input (sampling step in pixels).
    /// </summary>
    /// <remarks>
    /// Typical values are (1,1). Components should be positive and non-zero to avoid degenerate sampling.
    /// Default: (1, 1).
    /// </remarks>
    public D2D_VECTOR_2F KernelUnitLength { get => (D2D_VECTOR_2F)GetPropertyValue(KernelUnitLengthProperty)!; set => SetPropertyValue(KernelUnitLengthProperty, value); }

    /// <summary>
    /// Gets or sets the resampling filter used when the effect reads from the source.
    /// </summary>
    /// <remarks>
    /// Default: <see cref="D2D1_POINTSPECULAR_SCALE_MODE.D2D1_POINTSPECULAR_SCALE_MODE_LINEAR"/>.
    /// </remarks>
    public D2D1_POINTSPECULAR_SCALE_MODE ScaleMode { get => (D2D1_POINTSPECULAR_SCALE_MODE)GetPropertyValue(ScaleModeProperty)!; set => SetPropertyValue(ScaleModeProperty, value); }
}
