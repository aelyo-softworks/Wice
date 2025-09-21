namespace Wice.Effects;

/// <summary>
/// Direct2D "Premultiply" effect (D2D1Premultiply).
/// Converts a straight-alpha image to premultiplied alpha by multiplying each RGB channel by the alpha channel.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1PremultiplyString)]
#else
[Guid(Constants.CLSID_D2D1PremultiplyString)]
#endif
public partial class PremultiplyEffect : EffectWithSource
{
}
