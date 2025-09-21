namespace Wice.Effects;

/// <summary>
/// Direct2D Scale effect wrapper (CLSID_D2D1Scale).
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1ScaleString)]
#else
[Guid(Constants.CLSID_D2D1ScaleString)]
#endif
public partial class ScaleEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for <see cref="Scale"/>: effect parameter at index 0, default (1,1).
    /// </summary>
    public static EffectProperty ScaleProperty { get; }
    /// <summary>
    /// Descriptor for <see cref="CenterPoint"/>: effect parameter at index 1, default (0,0).
    /// </summary>
    public static EffectProperty CenterPointProperty { get; }
    /// <summary>
    /// Descriptor for <see cref="InterpolationMode"/>: effect parameter at index 2, default Linear.
    /// </summary>
    public static EffectProperty InterpolationModeProperty { get; }
    /// <summary>
    /// Descriptor for <see cref="BorderMode"/>: effect parameter at index 3, default Soft.
    /// </summary>
    public static EffectProperty BorderModeProperty { get; }
    /// <summary>
    /// Descriptor for <see cref="Sharpness"/>: effect parameter at index 4, default 0.
    /// </summary>
    public static EffectProperty SharpnessProperty { get; }

    static ScaleEffect()
    {
        ScaleProperty = EffectProperty.Add(typeof(ScaleEffect), nameof(Scale), 0, new D2D_VECTOR_2F(1f, 1f));
        CenterPointProperty = EffectProperty.Add(typeof(ScaleEffect), nameof(CenterPoint), 1, new D2D_VECTOR_2F(0f, 0f));
        InterpolationModeProperty = EffectProperty.Add(typeof(ScaleEffect), nameof(InterpolationMode), 2, D2D1_SCALE_INTERPOLATION_MODE.D2D1_SCALE_INTERPOLATION_MODE_LINEAR);
        BorderModeProperty = EffectProperty.Add(typeof(ScaleEffect), nameof(BorderMode), 3, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
        SharpnessProperty = EffectProperty.Add(typeof(ScaleEffect), nameof(Sharpness), 4, 0f);
    }

    /// <summary>
    /// Gets or sets the scaling factors to apply on X and Y.
    /// </summary>
    /// <value>
    /// A <see cref="D2D_VECTOR_2F"/> where X scales horizontally and Y scales vertically.
    /// Default is (1,1) (no scaling).
    /// </value>
    public D2D_VECTOR_2F Scale { get => (D2D_VECTOR_2F)GetPropertyValue(ScaleProperty)!; set => SetPropertyValue(ScaleProperty, value); }

    /// <summary>
    /// Gets or sets the center point around which scaling occurs, in input pixel coordinates.
    /// </summary>
    /// <value>
    /// A <see cref="D2D_VECTOR_2F"/> representing the origin of scaling. Default is (0,0) (top-left).
    /// </value>
    public D2D_VECTOR_2F CenterPoint { get => (D2D_VECTOR_2F)GetPropertyValue(CenterPointProperty)!; set => SetPropertyValue(CenterPointProperty, value); }

    /// <summary>
    /// Gets or sets the sampling method used when resizing.
    /// </summary>
    /// <value>
    /// A <see cref="D2D1_SCALE_INTERPOLATION_MODE"/> value. Default is <see cref="D2D1_SCALE_INTERPOLATION_MODE.D2D1_SCALE_INTERPOLATION_MODE_LINEAR"/>.
    /// </value>
    public D2D1_SCALE_INTERPOLATION_MODE InterpolationMode { get => (D2D1_SCALE_INTERPOLATION_MODE)GetPropertyValue(InterpolationModeProperty)!; set => SetPropertyValue(InterpolationModeProperty, value); }

    /// <summary>
    /// Gets or sets how areas outside the input bounds are sampled.
    /// </summary>
    /// <value>
    /// A <see cref="D2D1_BORDER_MODE"/> value. Default is <see cref="D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT"/>.
    /// </value>
    public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty)!; set => SetPropertyValue(BorderModeProperty, value); }

    /// <summary>
    /// Gets or sets the sharpness when downscaling.
    /// </summary>
    /// <value>
    /// A value in the [0,1] range, where 0 is smoothest and 1 is sharpest. Default is 0.
    /// </value>
    public float Sharpness { get => (float)GetPropertyValue(SharpnessProperty)!; set => SetPropertyValue(SharpnessProperty, value.Clamp(0f, 1f)); }
}
