namespace Wice.Effects;

/// <summary>
/// Wraps the Direct2D ConvolveMatrix effect (CLSID_D2D1ConvolveMatrix).
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1ConvolveMatrixString)]
#else
[Guid(Constants.CLSID_D2D1ConvolveMatrixString)]
#endif
public partial class ConvolveMatrixEffect : EffectWithSource
{
    /// <summary>
    /// Effect property descriptor for <see cref="KernelUnitLength"/> (Index 0, DIRECT mapping).
    /// Default: (1,1) DIPs.
    /// </summary>
    public static EffectProperty KernelUnitLengthProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="ScaleMode"/> (Index 1, DIRECT mapping).
    /// Default: <see cref="D2D1_CONVOLVEMATRIX_SCALE_MODE.D2D1_CONVOLVEMATRIX_SCALE_MODE_LINEAR"/>.
    /// </summary>
    public static EffectProperty ScaleModeProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="KernelSizeX"/> (Index 2, DIRECT mapping).
    /// Default: 3.
    /// </summary>
    public static EffectProperty KernelSizeXProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="KernelSizeY"/> (Index 3, DIRECT mapping).
    /// Default: 3.
    /// </summary>
    public static EffectProperty KernelSizeYProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="KernelMatrix"/> (Index 4, DIRECT mapping).
    /// Default: 3x3 identity kernel (center = 1).
    /// </summary>
    public static EffectProperty KernelMatrixProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="Divisor"/> (Index 5, DIRECT mapping).
    /// Default: 1.0f.
    /// </summary>
    public static EffectProperty DivisorProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="Bias"/> (Index 6, DIRECT mapping).
    /// Default: 0.0f.
    /// </summary>
    public static EffectProperty BiasProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="KernelOffset"/> (Index 7, DIRECT mapping).
    /// Default: (0,0).
    /// </summary>
    public static EffectProperty KernelOffsetProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="PreserveAlpha"/> (Index 8, DIRECT mapping).
    /// Default: false.
    /// </summary>
    public static EffectProperty PreserveAlphaProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="BorderMode"/> (Index 9, DIRECT mapping).
    /// Default: <see cref="D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT"/>.
    /// </summary>
    public static EffectProperty BorderModeProperty { get; }
    /// <summary>
    /// Effect property descriptor for <see cref="ClampOutput"/> (Index 10, DIRECT mapping).
    /// Default: false.
    /// </summary>
    public static EffectProperty ClampOutputProperty { get; }

    /// <summary>
    /// Registers effect property descriptors and default values for the ConvolveMatrix effect.
    /// </summary>
    static ConvolveMatrixEffect()
    {
        KernelUnitLengthProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(KernelUnitLength), 0, new D2D_VECTOR_2F(1f, 1f));
        ScaleModeProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(ScaleMode), 1, D2D1_CONVOLVEMATRIX_SCALE_MODE.D2D1_CONVOLVEMATRIX_SCALE_MODE_LINEAR);
        KernelSizeXProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(KernelSizeX), 2, 3u);
        KernelSizeYProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(KernelSizeY), 3, 3u);
        KernelMatrixProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(KernelMatrix), 4, new float[] { 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f });
        DivisorProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(Divisor), 5, 1f);
        BiasProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(Bias), 6, 0f);
        KernelOffsetProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(KernelOffset), 7, new D2D_VECTOR_2F());
        PreserveAlphaProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(PreserveAlpha), 8, false);
        BorderModeProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(BorderMode), 9, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
        ClampOutputProperty = EffectProperty.Add(typeof(ConvolveMatrixEffect), nameof(ClampOutput), 10, false);
    }

    /// <summary>
    /// Gets or sets the sampling distance in device-independent pixels between adjacent kernel samples.
    /// </summary>
    public D2D_VECTOR_2F KernelUnitLength { get => (D2D_VECTOR_2F)GetPropertyValue(KernelUnitLengthProperty)!; set => SetPropertyValue(KernelUnitLengthProperty, value); }

    /// <summary>
    /// Gets or sets the scaling mode used when sampling texels for the convolution.
    /// </summary>
    public D2D1_CONVOLVEMATRIX_SCALE_MODE ScaleMode { get => (D2D1_CONVOLVEMATRIX_SCALE_MODE)GetPropertyValue(ScaleModeProperty)!; set => SetPropertyValue(ScaleModeProperty, value); }

    /// <summary>
    /// Gets or sets the horizontal kernel size (number of columns).
    /// </summary>
    public uint KernelSizeX { get => (uint)GetPropertyValue(KernelSizeXProperty)!; set => SetPropertyValue(KernelSizeXProperty, value); }

    /// <summary>
    /// Gets or sets the vertical kernel size (number of rows).
    /// </summary>
    public uint KernelSizeY { get => (uint)GetPropertyValue(KernelSizeYProperty)!; set => SetPropertyValue(KernelSizeYProperty, value); }

    /// <summary>
    /// Gets or sets the flattened convolution kernel values in row-major order.
    /// </summary>
    public float[] KernelMatrix { get => (float[])GetPropertyValue(KernelMatrixProperty)!; set => SetPropertyValue(KernelMatrixProperty, value); }

    /// <summary>
    /// Gets or sets the divisor applied to the weighted sum of the kernel before bias is added.
    /// </summary>
    public float Divisor { get => (float)GetPropertyValue(DivisorProperty)!; set => SetPropertyValue(DivisorProperty, value); }

    /// <summary>
    /// Gets or sets the value added to the result after dividing by <see cref="Divisor"/>.
    /// </summary>
    public float Bias { get => (float)GetPropertyValue(BiasProperty)!; set => SetPropertyValue(BiasProperty, value); }

    /// <summary>
    /// Gets or sets the kernel center offset in pixels relative to the top-left of the kernel matrix.
    /// </summary>
    public D2D_VECTOR_2F KernelOffset { get => (D2D_VECTOR_2F)GetPropertyValue(KernelOffsetProperty)!; set => SetPropertyValue(KernelOffsetProperty, value); }

    /// <summary>
    /// Gets or sets whether alpha is preserved (not convolved).
    /// </summary>
    public bool PreserveAlpha { get => (bool)GetPropertyValue(PreserveAlphaProperty)!; set => SetPropertyValue(PreserveAlphaProperty, value); }

    /// <summary>
    /// Gets or sets the border sampling mode used when accessing pixels outside the source bounds.
    /// </summary>
    public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty)!; set => SetPropertyValue(BorderModeProperty, value); }

    /// <summary>
    /// Gets or sets whether the output color values are clamped to the [0,1] range.
    /// </summary>
    public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty)!; set => SetPropertyValue(ClampOutputProperty, value); }
}
