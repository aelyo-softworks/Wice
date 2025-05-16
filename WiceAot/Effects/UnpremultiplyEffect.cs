namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1UnPremultiplyString)]
#else
[Guid(Constants.CLSID_D2D1UnPremultiplyString)]
#endif
public partial class UnpremultiplyEffect : EffectWithSource
{
}
