namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1AlphaMaskString)]
#else
[Guid(Constants.CLSID_D2D1AlphaMaskString)]
#endif
public partial class AlphaMaskEffect : EffectWithSource
{
    public AlphaMaskEffect()
        : base(2)
    {
    }

    public IGraphicsEffectSource? AlphaMask { get => GetSource(1); set => SetSource(1, value); }
}
