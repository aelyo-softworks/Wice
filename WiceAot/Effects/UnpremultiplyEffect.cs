namespace Wice.Effects;

/// <summary>
/// Direct2D UnPremultiply effect.
/// Converts an image with premultiplied alpha into straight (unpremultiplied) alpha by dividing color channels by alpha.
/// </summary>
/// <remarks>
/// - Requires at least one <see cref="IGraphicsEffectSource"/> (provided by <see cref="EffectWithSource"/>).
/// - The effect corresponds to the native D2D effect CLSID "CLSID_D2D1UnPremultiply".
/// - The CLSID string is selected via conditional compilation to support both .NET Framework and .NET (Core/5+).
/// - Typical usage: feed a premultiplied alpha source to obtain straight alpha output for further processing.
/// </remarks>
/// <seealso cref="EffectWithSource"/>
/// <seealso cref="IGraphicsEffectSource"/>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1UnPremultiplyString)]
#else
[Guid(Constants.CLSID_D2D1UnPremultiplyString)]
#endif
public partial class UnpremultiplyEffect : EffectWithSource
{
}
