namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1LuminanceToAlphaString)]
#else
[Guid(Constants.CLSID_D2D1LuminanceToAlphaString)]
#endif
public partial class LuminanceToAlphaEffect : EffectWithSource
{
}
