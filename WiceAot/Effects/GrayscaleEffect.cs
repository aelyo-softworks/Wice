namespace Wice.Effects;

/// <summary>
/// Represents an effect that converts the colors of an image to grayscale.
/// </summary>
/// <remarks>This effect processes the input image and removes all color information,  resulting in an output
/// image that contains only shades of gray.  It can be used in scenarios where a monochromatic representation of an
/// image is required.</remarks>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1GrayscaleString)]
#else
[Guid(Constants.CLSID_D2D1GrayscaleString)]
#endif
public partial class GrayscaleEffect : EffectWithSource
{
}
