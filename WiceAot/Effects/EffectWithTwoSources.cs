namespace Wice.Effects;

/// <summary>
/// Base class for effects that require at least two input sources.
/// </summary>
/// <remarks>
/// Provides convenient accessors for the first two inputs via <see cref="Source1"/> (index 0)
/// and <see cref="Source2"/> (index 1). Additional inputs, if supported by a derived effect,
/// can be managed through the base <see cref="Effect"/> API.
/// </remarks>
public abstract partial class EffectWithTwoSources : Effect
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectWithTwoSources"/> class.
    /// </summary>
    /// <param name="sourcesCount">The total number of sources supported by the effect. Must be at least 2.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="sourcesCount"/> is less than 2.</exception>
    protected EffectWithTwoSources(uint sourcesCount = 2)
        : base(sourcesCount)
    {
        if (sourcesCount < 2)
            throw new ArgumentOutOfRangeException(nameof(sourcesCount));
    }

    /// <summary>
    /// Gets or sets the first input source (internal index 0).
    /// </summary>
    /// <remarks>
    /// Returns <see langword="null"/> if the source is not set. Setting <see langword="null"/> clears the input.
    /// Uses <see cref="Effect.GetSource(int)"/> and <see cref="Effect.SetSource(int, IGraphicsEffectSource)"/> internally.
    /// </remarks>
    public IGraphicsEffectSource? Source1 { get => GetSource(0); set => SetSource(0, value); }

    /// <summary>
    /// Gets or sets the second input source (internal index 1).
    /// </summary>
    /// <remarks>
    /// Returns <see langword="null"/> if the source is not set. Setting <see langword="null"/> clears the input.
    /// Uses <see cref="Effect.GetSource(int)"/> and <see cref="Effect.SetSource(int, IGraphicsEffectSource)"/> internally.
    /// </remarks>
    public IGraphicsEffectSource? Source2 { get => GetSource(1); set => SetSource(1, value); }
}
