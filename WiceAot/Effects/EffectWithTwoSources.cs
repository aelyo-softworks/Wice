namespace Wice.Effects;

/// <summary>
/// Base class for effects that require at least two input sources.
/// </summary>
public abstract partial class EffectWithTwoSources : Effect
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectWithTwoSources"/> class.
    /// </summary>
    /// <param name="sourcesCount">The total number of sources supported by the effect. Must be at least 2.</param>
    protected EffectWithTwoSources(uint sourcesCount = 2)
        : base(sourcesCount)
    {
        if (sourcesCount < 2)
            throw new ArgumentOutOfRangeException(nameof(sourcesCount));
    }

    /// <summary>
    /// Gets or sets the first input source (internal index 0).
    /// </summary>
    public IGraphicsEffectSource? Source1 { get => GetSource(0); set => SetSource(0, value); }

    /// <summary>
    /// Gets or sets the second input source (internal index 1).
    /// </summary>
    public IGraphicsEffectSource? Source2 { get => GetSource(1); set => SetSource(1, value); }
}
