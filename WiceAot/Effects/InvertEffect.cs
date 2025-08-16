namespace Wice.Effects;

/// <summary>
/// Represents an effect that inverts the colors of the input image.
/// </summary>
/// <remarks>This effect processes the input image by inverting its colors, producing an output where each pixel's
/// color is the complement of the corresponding input pixel. It can be used in scenarios where color inversion is
/// required, such as creating visual effects or analyzing image data.</remarks>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1InvertString)]
#else
[Guid(Constants.CLSID_D2D1InvertString)]
#endif
public partial class InvertEffect : EffectWithSource
{
}
