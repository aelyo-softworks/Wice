namespace Wice.Effects;

/// <summary>
/// Direct2D PointDiffuse lighting effect.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1PointDiffuseString)]
#else
[Guid(Constants.CLSID_D2D1PointDiffuseString)]
#endif
public partial class PointDiffuseEffect : EffectWithSource
{
    /// <summary>
    /// Effect property descriptor for <see cref="LightPosition"/> (index 0). Default: (0, 0, 0).
    /// </summary>
    public static EffectProperty LightPositionProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="DiffuseConstant"/> (index 1). Default: 1.0.
    /// </summary>
    public static EffectProperty DiffuseConstantProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="SurfaceScale"/> (index 2). Default: 1.0.
    /// </summary>
    public static EffectProperty SurfaceScaleProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="Color"/> (index 3). Mapping: COLOR_TO_VECTOR3. Default: (1, 1, 1).
    /// </summary>
    public static EffectProperty ColorProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="KernelUnitLength"/> (index 4). Default: (1, 1).
    /// </summary>
    public static EffectProperty KernelUnitLengthProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="ScaleMode"/> (index 5). Default: Linear.
    /// </summary>
    public static EffectProperty ScaleModeProperty { get; }

    static PointDiffuseEffect()
    {
        LightPositionProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(LightPosition), 0, new D2D_VECTOR_3F());
        DiffuseConstantProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(DiffuseConstant), 1, 1f);
        SurfaceScaleProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(SurfaceScale), 2, 1f);
        ColorProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(Color), 3, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3, new D2D_VECTOR_3F(1f, 1f, 1f));
        KernelUnitLengthProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(KernelUnitLength), 4, new D2D_VECTOR_2F(1f, 1f));
        ScaleModeProperty = EffectProperty.Add(typeof(PointDiffuseEffect), nameof(ScaleMode), 5, D2D1_POINTDIFFUSE_SCALE_MODE.D2D1_POINTDIFFUSE_SCALE_MODE_LINEAR);
    }

    /// <summary>
    /// Gets or sets the point light 3D position (X, Y, Z) in DIPs (Z in DIPs as well).
    /// </summary>
    public D2D_VECTOR_3F LightPosition { get => (D2D_VECTOR_3F)GetPropertyValue(LightPositionProperty)!; set => SetPropertyValue(LightPositionProperty, value); }

    /// <summary>
    /// Gets or sets the diffuse reflectance constant.
    /// </summary>
    public float DiffuseConstant { get => (float)GetPropertyValue(DiffuseConstantProperty)!; set => SetPropertyValue(DiffuseConstantProperty, value.Clamp(0f, 10000f)); }

    /// <summary>
    /// Gets or sets the height-to-normal scale factor applied when deriving surface normals from the input.
    /// </summary>
    public float SurfaceScale { get => (float)GetPropertyValue(SurfaceScaleProperty)!; set => SetPropertyValue(SurfaceScaleProperty, value.Clamp(0f, 10000f)); }

    /// <summary>
    /// Gets or sets the RGB color of the light as a 3-component vector.
    /// </summary>
    public D2D_VECTOR_3F Color { get => (D2D_VECTOR_3F)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }

    /// <summary>
    /// Gets or sets the kernel unit length in DIPs for X and Y used when computing image gradients.
    /// </summary>
    public D2D_VECTOR_2F KernelUnitLength { get => (D2D_VECTOR_2F)GetPropertyValue(KernelUnitLengthProperty)!; set => SetPropertyValue(KernelUnitLengthProperty, value); }

    /// <summary>
    /// Gets or sets the sampling mode used when reading the source.
    /// </summary>
    public D2D1_POINTDIFFUSE_SCALE_MODE ScaleMode { get => (D2D1_POINTDIFFUSE_SCALE_MODE)GetPropertyValue(ScaleModeProperty)!; set => SetPropertyValue(ScaleModeProperty, value); }
}
