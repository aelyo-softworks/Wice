namespace Wice.Effects;

/// <summary>
/// Wraps the Direct2D AlphaMask effect (D2D1_ALPHA_MASK).
/// Uses the alpha channel of the <see cref="AlphaMask"/> input (index 1)
/// to mask the primary <see cref="EffectWithSource.Source"/> input (index 0).
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1AlphaMaskString)]
#else
[Guid(Constants.CLSID_D2D1AlphaMaskString)]
#endif
public partial class AlphaMaskEffect : EffectWithSource
{
    /// <summary>
    /// Initializes a new instance of <see cref="AlphaMaskEffect"/> configured for two inputs.
    /// </summary>
    public AlphaMaskEffect()
        : base(2)
    {
    }

    /// <summary>
    /// Gets or sets the alpha mask input (index 1).
    /// The alpha channel of this input is used to mask the primary <see cref="EffectWithSource.Source"/>.
    /// </summary>
    /// <value>
    /// An <see cref="IGraphicsEffectSource"/> providing the mask; may be <see langword="null"/>.
    /// </value>
    public IGraphicsEffectSource? AlphaMask { get => GetSource(1); set => SetSource(1, value); }
}
