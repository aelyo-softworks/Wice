namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1GrayscaleString)]
#else
[Guid(Constants.CLSID_D2D1GrayscaleString)]
#endif
public partial class GrayscaleEffect : EffectWithSource
{
}
