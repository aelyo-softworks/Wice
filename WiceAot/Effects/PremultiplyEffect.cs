namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1PremultiplyString)]
#else
[Guid(Constants.CLSID_D2D1PremultiplyString)]
#endif
public partial class PremultiplyEffect : EffectWithSource
{
}
