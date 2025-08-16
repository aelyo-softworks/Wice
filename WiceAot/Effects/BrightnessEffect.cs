namespace Wice.Effects;

/// <summary>
/// Wraps the built-in Direct2D Brightness effect (CLSID_D2D1Brightness) for use in effect graphs.
/// </summary>
/// <remarks>
/// - Remaps input luminance using configurable black and white points to control perceived brightness.
/// - Exposes two parameters:
///   - <see cref="BlackPoint"/> (index 1): lower bound of the input range [0, 1].
///   - <see cref="WhitePoint"/> (index 0): upper bound of the input range [0, 1].
/// - Values are provided as <c>D2D_VECTOR_2F</c>, matching the underlying D2D signature.
/// </remarks>
/// <seealso cref="EffectWithSource"/>
/// <seealso cref="EffectProperty"/>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1BrightnessString)]
#else
[Guid(Constants.CLSID_D2D1BrightnessString)]
#endif
public partial class BrightnessEffect : EffectWithSource
{
    /// <summary>
    /// Effect property descriptor for <see cref="WhitePoint"/>.
    /// </summary>
    /// <remarks>
    /// - Index: 0 (D2D1_BRIGHTNESS_PROP_WHITE_POINT)
    /// - Type: <c>D2D_VECTOR_2F</c>
    /// - Default: <c>(1.0, 1.0)</c>
    /// - Mapping: <c>DIRECT</c>
    /// </remarks>
    public static EffectProperty WhitePointProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="BlackPoint"/>.
    /// </summary>
    /// <remarks>
    /// - Index: 1 (D2D1_BRIGHTNESS_PROP_BLACK_POINT)
    /// - Type: <c>D2D_VECTOR_2F</c>
    /// - Default: <c>(0.0, 0.0)</c>
    /// - Mapping: <c>DIRECT</c>
    /// </remarks>
    public static EffectProperty BlackPointProperty { get; }

    static BrightnessEffect()
    {
        WhitePointProperty = EffectProperty.Add(typeof(BrightnessEffect), nameof(WhitePoint), 0, new D2D_VECTOR_2F(1f, 1f));
        BlackPointProperty = EffectProperty.Add(typeof(BrightnessEffect), nameof(BlackPoint), 1, new D2D_VECTOR_2F());
    }

    /// <summary>
    /// Gets or sets the upper luminance bound used by the brightness remap.
    /// </summary>
    /// <remarks>
    /// - Expected range: [0, 1] per channel.
    /// - Default: <c>(1.0, 1.0)</c>.
    /// - Typically, <see cref="BlackPoint"/> should be less than <see cref="WhitePoint"/> component-wise.
    /// </remarks>
    public D2D_VECTOR_2F WhitePoint
    {
        get => (D2D_VECTOR_2F)GetPropertyValue(WhitePointProperty)!;
        set => SetPropertyValue(WhitePointProperty, value);
    }

    /// <summary>
    /// Gets or sets the lower luminance bound used by the brightness remap.
    /// </summary>
    /// <remarks>
    /// - Expected range: [0, 1] per channel.
    /// - Default: <c>(0.0, 0.0)</c>.
    /// - Typically, <see cref="BlackPoint"/> should be less than <see cref="WhitePoint"/> component-wise.
    /// </remarks>
    public D2D_VECTOR_2F BlackPoint
    {
        get => (D2D_VECTOR_2F)GetPropertyValue(BlackPointProperty)!;
        set => SetPropertyValue(BlackPointProperty, value);
    }
}
