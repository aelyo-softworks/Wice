namespace Wice.Effects;

#if NETFRAMEWORK
/// <summary>
/// Direct2D CLSID for the ConvolveMatrix effect (Framework).
/// </summary>
[Guid(D2D1Constants.CLSID_D2D1ConvolveMatrixString)]
#else
/// <summary>
/// Direct2D CLSID for the ConvolveMatrix effect (.NET).
/// </summary>
[Guid(Constants.CLSID_D2D1ConvolveMatrixString)]
#endif
/// <summary>
/// Wraps the Direct2D ConvolveMatrix effect (CLSID_D2D1ConvolveMatrix).
/// </summary>
/// <remarks>
/// - Provides a single input source (see <see cref="EffectWithSource.Source"/>).
/// - Exposes effect parameters through strongly-typed properties backed by <see cref="EffectProperty"/> descriptors.
/// - Property indices match the Direct2D property order expected by the effect interop.
/// - Default values match Direct2D defaults unless noted.
/// 
/// The convolution kernel is defined by <see cref="KernelSizeX"/>, <see cref="KernelSizeY"/>,
/// and the flattened <see cref="KernelMatrix"/> values (row-major order, length = X * Y).
/// The result is scaled by <see cref="Divisor"/> and offset by <see cref="Bias"/>.
/// Border sampling is controlled by <see cref="BorderMode"/> and output clamping by <see cref="ClampOutput"/>.
/// </remarks>
/// <seealso href="https://learn.microsoft.com/windows/win32/direct2d/convolvematrix-effect">ConvolveMatrix effect (Direct2D)</seealso>
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
    /// <remarks>
    /// - Maps to D2D property KERNEL_UNIT_LENGTH.
    /// - Default: (1,1) means per-pixel sampling.
    /// </remarks>
    public D2D_VECTOR_2F KernelUnitLength { get => (D2D_VECTOR_2F)GetPropertyValue(KernelUnitLengthProperty)!; set => SetPropertyValue(KernelUnitLengthProperty, value); }

    /// <summary>
    /// Gets or sets the scaling mode used when sampling texels for the convolution.
    /// </summary>
    /// <remarks>
    /// - Maps to D2D property SCALE_MODE.
    /// - Default: Linear sampling.
    /// </remarks>
    public D2D1_CONVOLVEMATRIX_SCALE_MODE ScaleMode { get => (D2D1_CONVOLVEMATRIX_SCALE_MODE)GetPropertyValue(ScaleModeProperty)!; set => SetPropertyValue(ScaleModeProperty, value); }

    /// <summary>
    /// Gets or sets the horizontal kernel size (number of columns).
    /// </summary>
    /// <remarks>
    /// - Maps to D2D property KERNEL_SIZE_X.
    /// - Default: 3.
    /// - Must match <c>KernelMatrix.Length == KernelSizeX * KernelSizeY</c> (not enforced at set-time).
    /// </remarks>
    public uint KernelSizeX { get => (uint)GetPropertyValue(KernelSizeXProperty)!; set => SetPropertyValue(KernelSizeXProperty, value); }

    /// <summary>
    /// Gets or sets the vertical kernel size (number of rows).
    /// </summary>
    /// <remarks>
    /// - Maps to D2D property KERNEL_SIZE_Y.
    /// - Default: 3.
    /// - Must match <c>KernelMatrix.Length == KernelSizeX * KernelSizeY</c> (not enforced at set-time).
    /// </remarks>
    public uint KernelSizeY { get => (uint)GetPropertyValue(KernelSizeYProperty)!; set => SetPropertyValue(KernelSizeYProperty, value); }

    /// <summary>
    /// Gets or sets the flattened convolution kernel values in row-major order.
    /// </summary>
    /// <remarks>
    /// - Maps to D2D property KERNEL_MATRIX.
    /// - Length must equal <see cref="KernelSizeX"/> * <see cref="KernelSizeY"/>.
    /// - Default: 3x3 identity kernel [0 0 0; 0 1 0; 0 0 0].
    /// </remarks>
    public float[] KernelMatrix { get => (float[])GetPropertyValue(KernelMatrixProperty)!; set => SetPropertyValue(KernelMatrixProperty, value); }

    /// <summary>
    /// Gets or sets the divisor applied to the weighted sum of the kernel before bias is added.
    /// </summary>
    /// <remarks>
    /// - Maps to D2D property DIVISOR.
    /// - Default: 1.0f.
    /// </remarks>
    public float Divisor { get => (float)GetPropertyValue(DivisorProperty)!; set => SetPropertyValue(DivisorProperty, value); }

    /// <summary>
    /// Gets or sets the value added to the result after dividing by <see cref="Divisor"/>.
    /// </summary>
    /// <remarks>
    /// - Maps to D2D property BIAS.
    /// - Default: 0.0f.
    /// </remarks>
    public float Bias { get => (float)GetPropertyValue(BiasProperty)!; set => SetPropertyValue(BiasProperty, value); }

    /// <summary>
    /// Gets or sets the kernel center offset in pixels relative to the top-left of the kernel matrix.
    /// </summary>
    /// <remarks>
    /// - Maps to D2D property KERNEL_OFFSET.
    /// - Default: (0,0). Typical center is (floor(X/2), floor(Y/2)).
    /// </remarks>
    public D2D_VECTOR_2F KernelOffset { get => (D2D_VECTOR_2F)GetPropertyValue(KernelOffsetProperty)!; set => SetPropertyValue(KernelOffsetProperty, value); }

    /// <summary>
    /// Gets or sets whether alpha is preserved (not convolved).
    /// </summary>
    /// <remarks>
    /// - Maps to D2D property PRESERVE_ALPHA.
    /// - When true, the effect only convolves color channels and copies the alpha from the source.
    /// - Default: false.
    /// </remarks>
    public bool PreserveAlpha { get => (bool)GetPropertyValue(PreserveAlphaProperty)!; set => SetPropertyValue(PreserveAlphaProperty, value); }

    /// <summary>
    /// Gets or sets the border sampling mode used when accessing pixels outside the source bounds.
    /// </summary>
    /// <remarks>
    /// - Maps to D2D property BORDER_MODE.
    /// - Default: <see cref="D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT"/>.
    /// </remarks>
    public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty)!; set => SetPropertyValue(BorderModeProperty, value); }

    /// <summary>
    /// Gets or sets whether the output color values are clamped to the [0,1] range.
    /// </summary>
    /// <remarks>
    /// - Maps to D2D property CLAMP_OUTPUT.
    /// - Default: false.
    /// </remarks>
    public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty)!; set => SetPropertyValue(ClampOutputProperty, value); }
}
