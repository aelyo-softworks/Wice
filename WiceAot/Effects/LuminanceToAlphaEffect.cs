namespace Wice.Effects;

/// <summary>
/// Represents an effect that converts the luminance of an image to an alpha channel.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1LuminanceToAlphaString)]
#else
[Guid(Constants.CLSID_D2D1LuminanceToAlphaString)]
#endif
public partial class LuminanceToAlphaEffect : EffectWithSource
{
}
