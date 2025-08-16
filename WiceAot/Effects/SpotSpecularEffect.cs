namespace Wice.Effects;

/// <summary>
/// Represents the Direct2D SpotSpecular lighting effect.
/// </summary>
/// <remarks>
/// - Models a spotlight shining on a height field and computes specular highlights.
/// - Properties are exposed as both <see cref="EffectProperty"/> descriptors (for D2D interop)
///   and CLR properties (for easy consumption).
/// - Numeric setters clamp values to documented ranges to match D2D semantics.
/// </remarks>
/// <seealso cref="EffectWithSource"/>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1SpotSpecularString)]
#else
[Guid(Constants.CLSID_D2D1SpotSpecularString)]
#endif
public partial class SpotSpecularEffect : EffectWithSource
{
    /// <summary>
    /// Effect property descriptor for <see cref="LightPosition"/>.
    /// </summary>
    public static EffectProperty LightPositionProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="PointsAt"/>.
    /// </summary>
    public static EffectProperty PointsAtProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="Focus"/>.
    /// </summary>
    public static EffectProperty FocusProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="LimitingConeAngle"/>.
    /// </summary>
    public static EffectProperty LimitingConeAngleProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="SpecularExponent"/>.
    /// </summary>
    public static EffectProperty SpecularExponentProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="SpecularConstant"/>.
    /// </summary>
    public static EffectProperty SpecularConstantProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="SurfaceScale"/>.
    /// </summary>
    public static EffectProperty SurfaceScaleProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="Color"/>.
    /// </summary>
    public static EffectProperty ColorProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="KernelUnitLength"/>.
    /// </summary>
    public static EffectProperty KernelUnitLengthProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="ScaleMode"/>.
    /// </summary>
    public static EffectProperty ScaleModeProperty { get; }

    // Registers effect properties with their indices, mappings, and defaults.
    static SpotSpecularEffect()
    {
        // Note: The underlying property registration uses effect parameter indices and mapping hints
        // matching the D2D spot specular effect contract.
        LightPositionProperty = EffectProperty.Add(typeof(SpotDiffuseEffect), nameof(LightPosition), 0, new D2D_VECTOR_3F());
        PointsAtProperty = EffectProperty.Add(typeof(SpotDiffuseEffect), nameof(PointsAt), 1, new D2D_VECTOR_3F());
        FocusProperty = EffectProperty.Add(typeof(SpotDiffuseEffect), nameof(Focus), 2, 1f);
        LimitingConeAngleProperty = EffectProperty.Add(typeof(SpotDiffuseEffect), nameof(LimitingConeAngle), 3, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 90f);
        SpecularExponentProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(SpecularExponent), 4, 1f);
        SpecularConstantProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(SpecularConstant), 5, 1f);
        SurfaceScaleProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(SurfaceScale), 6, 1f);
        ColorProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(Color), 7, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3, new D2D_VECTOR_3F(1f, 1f, 1f));
        KernelUnitLengthProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(KernelUnitLength), 8, new D2D_VECTOR_2F(1f, 1f));
        ScaleModeProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(ScaleMode), 9, D2D1_POINTDIFFUSE_SCALE_MODE.D2D1_POINTDIFFUSE_SCALE_MODE_LINEAR);
    }

    /// <summary>
    /// Gets or sets the 3D position of the spotlight in DIP units (x, y) and z-depth.
    /// </summary>
    /// <value>Default is (0, 0, 0).</value>
    public D2D_VECTOR_3F LightPosition { get => (D2D_VECTOR_3F)GetPropertyValue(LightPositionProperty)!; set => SetPropertyValue(LightPositionProperty, value); }

    /// <summary>
    /// Gets or sets the 3D point that the light aims at (the spotlight target).
    /// </summary>
    /// <value>Default is (0, 0, 0).</value>
    public D2D_VECTOR_3F PointsAt { get => (D2D_VECTOR_3F)GetPropertyValue(PointsAtProperty)!; set => SetPropertyValue(PointsAtProperty, value); }

    /// <summary>
    /// Gets or sets the spotlight focus. Higher values create a tighter beam.
    /// </summary>
    /// <remarks>Clamped to the range [0, 200].</remarks>
    /// <value>Default is 1.0.</value>
    public float Focus { get => (float)GetPropertyValue(FocusProperty)!; set => SetPropertyValue(FocusProperty, value.Clamp(0f, 200f)); }

    /// <summary>
    /// Gets or sets the limiting cone angle in degrees for the spotlight.
    /// Pixels outside this angle relative to <see cref="PointsAt"/> are not lit.
    /// </summary>
    /// <remarks>Clamped to the range [0, 90].</remarks>
    /// <value>Default is 90 degrees.</value>
    public float LimitingConeAngle { get => (float)GetPropertyValue(LimitingConeAngleProperty)!; set => SetPropertyValue(LimitingConeAngleProperty, value.Clamp(0f, 90f)); }

    /// <summary>
    /// Gets or sets the specular exponent (shininess). Larger values produce smaller, sharper highlights.
    /// </summary>
    /// <remarks>Clamped to the range [1, 128].</remarks>
    /// <value>Default is 1.0.</value>
    public float SpecularExponent { get => (float)GetPropertyValue(SpecularExponentProperty)!; set => SetPropertyValue(SpecularExponentProperty, value.Clamp(1f, 128f)); }

    /// <summary>
    /// Gets or sets the specular constant (intensity multiplier).
    /// </summary>
    /// <remarks>Clamped to the range [0, 10000].</remarks>
    /// <value>Default is 1.0.</value>
    public float SpecularConstant { get => (float)GetPropertyValue(SpecularConstantProperty)!; set => SetPropertyValue(SpecularConstantProperty, value.Clamp(0f, 10000f)); }

    /// <summary>
    /// Gets or sets the height scale of the surface's height map.
    /// </summary>
    /// <remarks>Clamped to the range [0, 10000].</remarks>
    /// <value>Default is 1.0.</value>
    public float SurfaceScale { get => (float)GetPropertyValue(SurfaceScaleProperty)!; set => SetPropertyValue(SurfaceScaleProperty, value.Clamp(0f, 10000f)); }

    /// <summary>
    /// Gets or sets the RGB color of the light as a vector (r, g, b) in the range [0, 1].
    /// </summary>
    /// <value>Default is (1, 1, 1) for white light.</value>
    public D2D_VECTOR_3F Color { get => (D2D_VECTOR_3F)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }

    /// <summary>
    /// Gets or sets the kernel unit length used for computing the gradient (normal) from the height map.
    /// </summary>
    /// <value>Default is (1, 1).</value>
    public D2D_VECTOR_2F KernelUnitLength { get => (D2D_VECTOR_2F)GetPropertyValue(KernelUnitLengthProperty)!; set => SetPropertyValue(KernelUnitLengthProperty, value); }

    /// <summary>
    /// Gets or sets the sampling mode for the height map when computing normals.
    /// </summary>
    /// <value>Default is <see cref="D2D1_POINTDIFFUSE_SCALE_MODE.D2D1_POINTDIFFUSE_SCALE_MODE_LINEAR"/>.</value>
    public D2D1_POINTDIFFUSE_SCALE_MODE ScaleMode { get => (D2D1_POINTDIFFUSE_SCALE_MODE)GetPropertyValue(ScaleModeProperty)!; set => SetPropertyValue(ScaleModeProperty, value); }
}
