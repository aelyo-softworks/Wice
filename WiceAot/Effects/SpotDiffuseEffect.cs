namespace Wice.Effects;

/// <summary>
/// Wraps the Direct2D SpotDiffuse effect, exposing all effect parameters as strongly-typed properties.
/// </summary>
/// <remarks>
/// - This effect models a spotlight that illuminates the input height field to produce a diffuse shading result.
/// - Properties are mapped to the underlying D2D property bag through <see cref="EffectProperty"/> descriptors,
///   using the indices defined by the D2D1_SPOTDIFFUSE spec.
/// - Some properties specify mapping hints (eg. radians-to-degrees, color-to-vector3) to match the native ABI.
/// - This effect requires a source (see <see cref="EffectWithSource.Source"/>).
/// </remarks>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1SpotDiffuseString)]
#else
[Guid(Constants.CLSID_D2D1SpotDiffuseString)]
#endif
public partial class SpotDiffuseEffect : EffectWithSource
{
    /// <summary>
    /// Effect property descriptor for <see cref="LightPosition"/> (index 0).
    /// </summary>
    public static EffectProperty LightPositionProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="PointsAt"/> (index 1).
    /// </summary>
    public static EffectProperty PointsAtProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="Focus"/> (index 2).
    /// </summary>
    public static EffectProperty FocusProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="LimitingConeAngle"/> (index 3).
    /// Uses mapping <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES"/>.
    /// </summary>
    public static EffectProperty LimitingConeAngleProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="DiffuseConstant"/> (index 4).
    /// </summary>
    public static EffectProperty DiffuseConstantProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="SurfaceScale"/> (index 5).
    /// </summary>
    public static EffectProperty SurfaceScaleProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="Color"/> (index 6).
    /// Uses mapping <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3"/>.
    /// </summary>
    public static EffectProperty ColorProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="KernelUnitLength"/> (index 7).
    /// </summary>
    public static EffectProperty KernelUnitLengthProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="ScaleMode"/> (index 8).
    /// </summary>
    public static EffectProperty ScaleModeProperty { get; }

    static SpotDiffuseEffect()
    {
        // Indices, mappings and defaults follow the D2D1_SPOTDIFFUSE parameter ordering.
        LightPositionProperty = EffectProperty.Add(typeof(SpotDiffuseEffect), nameof(LightPosition), 0, new D2D_VECTOR_3F());
        PointsAtProperty = EffectProperty.Add(typeof(SpotDiffuseEffect), nameof(PointsAt), 1, new D2D_VECTOR_3F());
        FocusProperty = EffectProperty.Add(typeof(SpotDiffuseEffect), nameof(Focus), 2, 1f);
        LimitingConeAngleProperty = EffectProperty.Add(typeof(SpotDiffuseEffect), nameof(LimitingConeAngle), 3, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES, 90f);

        // Note: These use the same indices and defaults as the D2D Spot/Point diffuse family.
        // Mapping is DIRECT unless otherwise specified.
        DiffuseConstantProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(DiffuseConstant), 4, 1f);
        SurfaceScaleProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(SurfaceScale), 5, 1f);
        ColorProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(Color), 6, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3, new D2D_VECTOR_3F(1f, 1f, 1f));
        KernelUnitLengthProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(KernelUnitLength), 7, new D2D_VECTOR_2F(1f, 1f));
        ScaleModeProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(ScaleMode), 8, D2D1_POINTDIFFUSE_SCALE_MODE.D2D1_POINTDIFFUSE_SCALE_MODE_LINEAR);
    }

    /// <summary>
    /// Gets or sets the 3D position of the spotlight in effect space.
    /// </summary>
    /// <value>XYZ coordinates of the light position.</value>
    public D2D_VECTOR_3F LightPosition { get => (D2D_VECTOR_3F)GetPropertyValue(LightPositionProperty)!; set => SetPropertyValue(LightPositionProperty, value); }

    /// <summary>
    /// Gets or sets the 3D point the spotlight is aimed at.
    /// </summary>
    /// <value>XYZ coordinates of the target point.</value>
    public D2D_VECTOR_3F PointsAt { get => (D2D_VECTOR_3F)GetPropertyValue(PointsAtProperty)!; set => SetPropertyValue(PointsAtProperty, value); }

    /// <summary>
    /// Gets or sets the spotlight focus exponent.
    /// </summary>
    /// <remarks>
    /// Higher values increase concentration at the center of the cone.
    /// The value is clamped to [0, 200].
    /// </remarks>
    public float Focus { get => (float)GetPropertyValue(FocusProperty)!; set => SetPropertyValue(FocusProperty, value.Clamp(0f, 200f)); }

    /// <summary>
    /// Gets or sets the limiting cone angle of the spotlight.
    /// </summary>
    /// <remarks>
    /// - Expressed in degrees for this API and clamped to [0, 90].
    /// - Serialized using mapping <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RADIANS_TO_DEGREES"/>.
    /// </remarks>
    public float LimitingConeAngle { get => (float)GetPropertyValue(LimitingConeAngleProperty)!; set => SetPropertyValue(LimitingConeAngleProperty, value.Clamp(0f, 90f)); }

    /// <summary>
    /// Gets or sets the diffuse reflection scaling constant.
    /// </summary>
    /// <remarks>Clamped to [0, 10000].</remarks>
    public float DiffuseConstant { get => (float)GetPropertyValue(DiffuseConstantProperty)!; set => SetPropertyValue(DiffuseConstantProperty, value.Clamp(0f, 10000f)); }

    /// <summary>
    /// Gets or sets the height scale of the input surface used for normal computation.
    /// </summary>
    /// <remarks>Clamped to [0, 10000].</remarks>
    public float SurfaceScale { get => (float)GetPropertyValue(SurfaceScaleProperty)!; set => SetPropertyValue(SurfaceScaleProperty, value.Clamp(0f, 10000f)); }

    /// <summary>
    /// Gets or sets the RGB color of the light.
    /// </summary>
    /// <remarks>
    /// Serialized using <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3"/>.
    /// </remarks>
    public D2D_VECTOR_3F Color { get => (D2D_VECTOR_3F)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }

    /// <summary>
    /// Gets or sets the kernel unit length for X/Y when sampling the height field.
    /// </summary>
    public D2D_VECTOR_2F KernelUnitLength { get => (D2D_VECTOR_2F)GetPropertyValue(KernelUnitLengthProperty)!; set => SetPropertyValue(KernelUnitLengthProperty, value); }

    /// <summary>
    /// Gets or sets the sampling scale mode used by the effect.
    /// </summary>
    public D2D1_POINTDIFFUSE_SCALE_MODE ScaleMode { get => (D2D1_POINTDIFFUSE_SCALE_MODE)GetPropertyValue(ScaleModeProperty)!; set => SetPropertyValue(ScaleModeProperty, value); }
}
