namespace Wice.Effects;

public abstract partial class EffectWithTwoSources : Effect
{
    protected EffectWithTwoSources(uint sourcesCount = 2)
        : base(sourcesCount)
    {
        if (sourcesCount < 2)
            throw new ArgumentOutOfRangeException(nameof(sourcesCount));
    }

    public IGraphicsEffectSource Source1 { get => GetSource(0); set => SetSource(0, value); }
    public IGraphicsEffectSource Source2 { get => GetSource(1); set => SetSource(1, value); }
}
