namespace Wice.Effects;

public abstract class EffectWithSource : Effect
{
    protected EffectWithSource(int sourcesCount = 1)
        : base(sourcesCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sourcesCount, 1);
    }

    public IGraphicsEffectSource Source { get => GetSource(0); set => SetSource(0, value); }
}
