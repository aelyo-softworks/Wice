namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1AlphaMaskString)]
public partial class AlphaMaskEffect : EffectWithSource
{
    public AlphaMaskEffect()
        : base(2)
    {
    }

    public IGraphicsEffectSource? AlphaMask { get => GetSource(1); set => SetSource(1, value); }
}
