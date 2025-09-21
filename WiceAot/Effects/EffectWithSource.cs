namespace Wice.Effects;

/// <summary>
/// Base class for effects that require at least one <see cref="IGraphicsEffectSource"/> input.
/// </summary>
public abstract partial class EffectWithSource : Effect
{
    /// <summary>
    /// Initializes a new instance of <see cref="EffectWithSource"/> with the specified number of input sources.
    /// </summary>
    /// <param name="sourcesCount">The maximum number of input sources this effect supports. Must be at least 1.</param>
    protected EffectWithSource(uint sourcesCount = 1)
        : base(sourcesCount)
    {
        if (sourcesCount < 1)
            throw new ArgumentOutOfRangeException(nameof(sourcesCount));
    }

    /// <summary>
    /// Gets or sets the primary input source (index 0) for this effect.
    /// </summary>
    public IGraphicsEffectSource? Source { get => GetSource(0); set => SetSource(0, value); }
}
