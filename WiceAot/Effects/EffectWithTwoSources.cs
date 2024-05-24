namespace Wice.Effects;

public abstract class EffectWithTwoSources : Effect
{
    protected EffectWithTwoSources(int sourcesCount = 2)
        : base(sourcesCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sourcesCount, 2);
    }

    public IGraphicsEffectSource Source1 { get => GetSource(0); set => SetSource(0, value); }
    public IGraphicsEffectSource Source2 { get => GetSource(1); set => SetSource(1, value); }
}
