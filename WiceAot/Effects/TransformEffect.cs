namespace Wice.Effects;

/// <summary>
/// Wraps the Direct2D 3D Transform effect (CLSID_D2D13DTransform).
/// </summary>
/// <remarks>
/// - Inherits <see cref="EffectWithSource"/> and therefore requires at least one input source.
/// - Exposes three effect parameters ordered as follows:
///   0: <see cref="InterpolationMode"/>,
///   1: <see cref="BorderMode"/>,
///   2: <see cref="TransformMatrix"/>.
/// - Defaults are LINEAR interpolation, SOFT border mode, and identity matrix.
/// - The CLSID is resolved via <see cref="GuidAttribute"/> with a conditional constant per target framework.
/// </remarks>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D13DTransformString)]
#else
[Guid(Constants.CLSID_D2D13DTransformString)]
#endif
public partial class TransformEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for <see cref="InterpolationMode"/> (effect parameter index 0).
    /// </summary>
    /// <remarks>Default: <see cref="D2D1_3DTRANSFORM_INTERPOLATION_MODE.D2D1_3DTRANSFORM_INTERPOLATION_MODE_LINEAR"/>.</remarks>
    public static EffectProperty InterpolationModeProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="BorderMode"/> (effect parameter index 1).
    /// </summary>
    /// <remarks>Default: <see cref="D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT"/>.</remarks>
    public static EffectProperty BorderModeProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="TransformMatrix"/> (effect parameter index 2).
    /// </summary>
    /// <remarks>Default: 4x4 identity matrix.</remarks>
    public static EffectProperty TransformMatrixProperty { get; }

    /// <summary>
    /// Registers effect properties and their defaults.
    /// </summary>
    static TransformEffect()
    {
        InterpolationModeProperty = EffectProperty.Add(typeof(TransformEffect), nameof(InterpolationMode), 0, D2D1_3DTRANSFORM_INTERPOLATION_MODE.D2D1_3DTRANSFORM_INTERPOLATION_MODE_LINEAR);
        BorderModeProperty = EffectProperty.Add(typeof(TransformEffect), nameof(BorderMode), 1, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
        TransformMatrixProperty = EffectProperty.Add(typeof(TransformEffect), nameof(TransformMatrix), 2, new D2D_MATRIX_4X4_F(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f));
    }

    /// <summary>
    /// Gets or sets the sampling strategy used when transforming the input.
    /// </summary>
    /// <remarks>Effect parameter index: 0. Default: LINEAR.</remarks>
    public D2D1_3DTRANSFORM_INTERPOLATION_MODE InterpolationMode
    {
        get => (D2D1_3DTRANSFORM_INTERPOLATION_MODE)GetPropertyValue(InterpolationModeProperty)!;
        set => SetPropertyValue(InterpolationModeProperty, value);
    }

    /// <summary>
    /// Gets or sets how samples are computed outside the input bounds.
    /// </summary>
    /// <remarks>Effect parameter index: 1. Default: SOFT.</remarks>
    public D2D1_BORDER_MODE BorderMode
    {
        get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty)!;
        set => SetPropertyValue(BorderModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the 4x4 transform matrix applied to the input.
    /// </summary>
    /// <remarks>Effect parameter index: 2. Default: Identity.</remarks>
    public D2D_MATRIX_4X4_F TransformMatrix
    {
        get => (D2D_MATRIX_4X4_F)GetPropertyValue(TransformMatrixProperty)!;
        set => SetPropertyValue(TransformMatrixProperty, value);
    }
}
