namespace Wice.Effects;

/// <summary>
/// Direct2D UnPremultiply effect.
/// Converts an image with premultiplied alpha into straight (unpremultiplied) alpha by dividing color channels by alpha.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1UnPremultiplyString)]
#else
[Guid(Constants.CLSID_D2D1UnPremultiplyString)]
#endif
public partial class UnpremultiplyEffect : EffectWithSource
{
}
