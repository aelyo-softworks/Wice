namespace Wice.Effects;

/// <summary>
/// Base class for effects that require at least one <see cref="IGraphicsEffectSource"/> input.
/// </summary>
/// <remarks>
/// - This type guarantees that the effect has at least one source by validating the constructor argument.
/// - The <see cref="Source"/> property provides a convenient accessor for the primary input at index 0.
/// - For multi-input effects, use <see cref="Effect.Sources"/> (in the base class) to access additional inputs.
/// </remarks>
/// <seealso cref="Effect"/>
/// <seealso cref="IGraphicsEffectSource"/>
public abstract partial class EffectWithSource : Effect
{
    /// <summary>
    /// Initializes a new instance of <see cref="EffectWithSource"/> with the specified number of input sources.
    /// </summary>
    /// <param name="sourcesCount">The maximum number of input sources this effect supports. Must be at least 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="sourcesCount"/> is less than 1.</exception>
    protected EffectWithSource(uint sourcesCount = 1)
        : base(sourcesCount)
    {
        if (sourcesCount < 1)
            throw new ArgumentOutOfRangeException(nameof(sourcesCount));
    }

    /// <summary>
    /// Gets or sets the primary input source (index 0) for this effect.
    /// </summary>
    /// <remarks>
    /// - Equivalent to <c>GetSource(0)</c> and <c>SetSource(0, value)</c>.
    /// - For effects with multiple inputs, configure additional inputs through <see cref="Effect.Sources"/>.
    /// </remarks>
    public IGraphicsEffectSource? Source { get => GetSource(0); set => SetSource(0, value); }
}
