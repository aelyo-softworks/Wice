namespace Wice.Effects;

public abstract partial class EffectWithSource : Effect
{
    protected EffectWithSource(uint sourcesCount = 1)
        : base(sourcesCount)
    {
        if (sourcesCount < 1)
            throw new ArgumentOutOfRangeException(nameof(sourcesCount));
    }

    public IGraphicsEffectSource? Source { get => GetSource(0); set => SetSource(0, value); }
}
