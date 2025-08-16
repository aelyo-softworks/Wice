namespace Wice.Effects;

/// <summary>
/// Direct2D ArithmeticComposite effect wrapper that blends two input sources using an arithmetic expression.
/// </summary>
/// <remarks>
/// - Requires at least two sources (see <see cref="EffectWithTwoSources"/>): <see cref="EffectWithTwoSources.Source1"/> and <see cref="EffectWithTwoSources.Source2"/>.<br/>
/// - Output is computed as:
///   Output = K1 * Source1 * Source2 + K2 * Source1 + K3 * Source2 + K4,
///   where the coefficients vector (K1, K2, K3, K4) is stored in <see cref="Coefficients"/>.<br/>
/// - When <see cref="ClampOutput"/> is true, the result is clamped to the [0, 1] range per channel.
/// - The CLSID is provided via the <see cref="GuidAttribute"/> above for D2D interop.
/// </remarks>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1ArithmeticCompositeString)]
#else
[Guid(Constants.CLSID_D2D1ArithmeticCompositeString)]
#endif
public partial class ArithmeticCompositeEffect : EffectWithTwoSources
{
    /// <summary>
    /// Descriptor for <see cref="Coefficients"/> (effect property index 0).
    /// </summary>
    /// <remarks>
    /// - Mapping: DIRECT to the underlying D2D property bag.<br/>
    /// - Default: (K1 = 1, K2 = 0, K3 = 0, K4 = 0), which passes through Source1 when Source2 is 1 or multiplies when both sources are present.<br/>
    /// - Used by the effect pipeline to serialize the coefficients into the D2D property bag.
    /// </remarks>
    public static EffectProperty CoefficientsProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="ClampOutput"/> (effect property index 1).
    /// </summary>
    /// <remarks>
    /// - Mapping: DIRECT to the underlying D2D property bag.<br/>
    /// - Default: false (no clamping). When true, output channels are clamped to [0, 1].
    /// </remarks>
    public static EffectProperty ClampOutputProperty { get; }

    // Registers the effect properties with their indices and defaults.
    static ArithmeticCompositeEffect()
    {
        CoefficientsProperty = EffectProperty.Add(typeof(ArithmeticCompositeEffect), nameof(Coefficients), 0, new D2D_VECTOR_4F(1f, 0f, 0f, 0f));
        ClampOutputProperty = EffectProperty.Add(typeof(ArithmeticCompositeEffect), nameof(ClampOutput), 1, false);
    }

    /// <summary>
    /// Gets or sets the arithmetic coefficients vector (K1, K2, K3, K4).
    /// </summary>
    /// <remarks>
    /// The output is computed as:
    /// Output = K1 * Source1 * Source2 + K2 * Source1 + K3 * Source2 + K4.
    /// </remarks>
    public D2D_VECTOR_4F Coefficients { get => (D2D_VECTOR_4F)GetPropertyValue(CoefficientsProperty)!; set => SetPropertyValue(CoefficientsProperty, value); }

    /// <summary>
    /// Gets or sets whether the effect output is clamped to the [0, 1] range per channel.
    /// </summary>
    /// <remarks>
    /// Default is false (no clamping).
    /// </remarks>
    public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty)!; set => SetPropertyValue(ClampOutputProperty, value); }
}
