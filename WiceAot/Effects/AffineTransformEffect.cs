namespace Wice.Effects;

/// <summary>
/// Direct2D 2D affine transform effect.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D12DAffineTransformString)]
#else
[Guid(Constants.CLSID_D2D12DAffineTransformString)]
#endif
public partial class AffineTransformEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for <see cref="InterpolationMode"/> (effect parameter index 0).
    /// Controls how the effect samples pixels when transforming.
    /// </summary>
    public static EffectProperty InterpolationModeProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="BorderMode"/> (effect parameter index 1).
    /// Defines how the effect treats pixels outside the input bounds.
    /// </summary>
    public static EffectProperty BorderModeProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="TransformMatrix"/> (effect parameter index 2).
    /// Specifies the 3x2 transform matrix to apply.
    /// </summary>
    public static EffectProperty TransformMatrixProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="Sharpness"/> (effect parameter index 3).
    /// Tweaks edge clarity during resampling (0..1).
    /// </summary>
    public static EffectProperty SharpnessProperty { get; }

    /// <summary>
    /// Registers effect properties and their default values.
    /// Order of registration defines the stable parameter indices used by D2D.
    /// </summary>
    static AffineTransformEffect()
    {
        InterpolationModeProperty = EffectProperty.Add(typeof(AffineTransformEffect), nameof(InterpolationMode), 0, D2D1_2DAFFINETRANSFORM_INTERPOLATION_MODE.D2D1_2DAFFINETRANSFORM_INTERPOLATION_MODE_LINEAR);
        BorderModeProperty = EffectProperty.Add(typeof(AffineTransformEffect), nameof(BorderMode), 1, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
        TransformMatrixProperty = EffectProperty.Add(typeof(AffineTransformEffect), nameof(TransformMatrix), 2, D2D_MATRIX_3X2_F.Identity());
        SharpnessProperty = EffectProperty.Add(typeof(AffineTransformEffect), nameof(Sharpness), 3, 1f);
    }

    /// <summary>
    /// Gets or sets the interpolation mode used when sampling the source during the transform.
    /// Default is <see cref="D2D1_2DAFFINETRANSFORM_INTERPOLATION_MODE.D2D1_2DAFFINETRANSFORM_INTERPOLATION_MODE_LINEAR"/>.
    /// </summary>
    public D2D1_2DAFFINETRANSFORM_INTERPOLATION_MODE InterpolationMode { get => (D2D1_2DAFFINETRANSFORM_INTERPOLATION_MODE)GetPropertyValue(InterpolationModeProperty)!; set => SetPropertyValue(InterpolationModeProperty, value); }

    /// <summary>
    /// Gets or sets the border mode that determines how pixels outside the input are sampled.
    /// Default is <see cref="D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT"/>.
    /// </summary>
    public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty)!; set => SetPropertyValue(BorderModeProperty, value); }

    /// <summary>
    /// Gets or sets the 3x2 transform matrix applied to the source.
    /// Default is <see cref="D2D_MATRIX_3X2_F.Identity()"/>.
    /// </summary>
    public D2D_MATRIX_3X2_F TransformMatrix { get => (D2D_MATRIX_3X2_F)GetPropertyValue(TransformMatrixProperty)!; set => SetPropertyValue(TransformMatrixProperty, value); }

    /// <summary>
    /// Gets or sets the sampling sharpness in the range [0, 1]. Values are clamped.
    /// Default is 1.0 (maximum sharpness).
    /// </summary>
    public float Sharpness { get => (float)GetPropertyValue(SharpnessProperty)!; set => SetPropertyValue(SharpnessProperty, value.Clamp(0f, 1f)); }
}
