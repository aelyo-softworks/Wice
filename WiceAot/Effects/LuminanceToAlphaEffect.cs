namespace Wice.Effects;

/// <summary>
/// Represents an effect that converts the luminance of an image to an alpha channel.
/// </summary>
/// <remarks>This effect processes the input image by mapping its luminance values to the alpha channel, 
/// effectively creating a grayscale transparency effect. The resulting image retains the original  color information in
/// the RGB channels, but the alpha channel is determined by the luminance.</remarks>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1LuminanceToAlphaString)]
#else
[Guid(Constants.CLSID_D2D1LuminanceToAlphaString)]
#endif
public partial class LuminanceToAlphaEffect : EffectWithSource
{
}
