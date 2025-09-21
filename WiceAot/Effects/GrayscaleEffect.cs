namespace Wice.Effects;

/// <summary>
/// Represents an effect that converts the colors of an image to grayscale.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1GrayscaleString)]
#else
[Guid(Constants.CLSID_D2D1GrayscaleString)]
#endif
public partial class GrayscaleEffect : EffectWithSource
{
}
