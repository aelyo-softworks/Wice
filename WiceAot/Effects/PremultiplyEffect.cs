namespace Wice.Effects;

/// <summary>
/// Direct2D "Premultiply" effect (D2D1Premultiply).
/// Converts a straight-alpha image to premultiplied alpha by multiplying each RGB channel by the alpha channel.
/// </summary>
/// <remarks>
/// - This effect exposes a single input through <see cref="EffectWithSource.Source"/> and has no configurable properties.
/// - The CLSID is provided via <see cref="GuidAttribute"/> and switches between Win2D/D2D constants depending on the target framework.
/// - Use this effect when downstream compositing or effects require premultiplied alpha.
/// </remarks>
/// <example>
/// <code>
/// var effect = new PremultiplyEffect
/// {
///     Source = bitmapOrEffect
/// };
/// // Use 'effect' as input to other effects or draw operations that expect premultiplied alpha.
/// </code>
/// </example>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1PremultiplyString)]
#else
[Guid(Constants.CLSID_D2D1PremultiplyString)]
#endif
public partial class PremultiplyEffect : EffectWithSource
{
}
