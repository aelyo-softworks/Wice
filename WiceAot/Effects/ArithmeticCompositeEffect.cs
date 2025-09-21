namespace Wice.Effects;

/// <summary>
/// Direct2D ArithmeticComposite effect wrapper that blends two input sources using an arithmetic expression.
/// </summary>
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
    public static EffectProperty CoefficientsProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="ClampOutput"/> (effect property index 1).
    /// </summary>
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
    public D2D_VECTOR_4F Coefficients { get => (D2D_VECTOR_4F)GetPropertyValue(CoefficientsProperty)!; set => SetPropertyValue(CoefficientsProperty, value); }

    /// <summary>
    /// Gets or sets whether the effect output is clamped to the [0, 1] range per channel.
    /// </summary>
    public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty)!; set => SetPropertyValue(ClampOutputProperty, value); }
}
