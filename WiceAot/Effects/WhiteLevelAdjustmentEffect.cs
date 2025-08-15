namespace Wice.Effects;

/// <summary>
/// Wraps the Direct2D WhiteLevelAdjustment effect (CLSID_D2D1WhiteLevelAdjustment).
/// </summary>
/// <remarks>
/// - Inherits <see cref="EffectWithSource"/>, therefore requires a primary <see cref="IGraphicsEffectSource"/>.
/// - Exposes two float parameters mapped to the D2D property bag:
///   0 = <see cref="InputWhiteLevel"/>, 1 = <see cref="OutputWhiteLevel"/>.
/// - Default values for both parameters are 0.0f as registered in the static constructor.
/// - The underlying effect may clamp values to its supported range (see D2D documentation).
/// </remarks>
/// <seealso cref="EffectWithSource"/>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1WhiteLevelAdjustmentString)]
#else
[Guid(Constants.CLSID_D2D1WhiteLevelAdjustmentString)]
#endif
public partial class WhiteLevelAdjustmentEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for the <see cref="InputWhiteLevel"/> parameter.
    /// </summary>
    /// <remarks>
    /// - Effect property index: 0
    /// - Default value: 0.0f
    /// </remarks>
    public static EffectProperty InputWhiteLevelProperty { get; }

    /// <summary>
    /// Descriptor for the <see cref="OutputWhiteLevel"/> parameter.
    /// </summary>
    /// <remarks>
    /// - Effect property index: 1
    /// - Default value: 0.0f
    /// </remarks>
    public static EffectProperty OutputWhiteLevelProperty { get; }

    static WhiteLevelAdjustmentEffect()
    {
        InputWhiteLevelProperty = EffectProperty.Add(typeof(WhiteLevelAdjustmentEffect), nameof(InputWhiteLevel), 0, 0f);
        OutputWhiteLevelProperty = EffectProperty.Add(typeof(WhiteLevelAdjustmentEffect), nameof(OutputWhiteLevel), 1, 0f);
    }

    /// <summary>
    /// Gets or sets the input white level value provided to the effect.
    /// </summary>
    /// <value>A floating-point value. The underlying D2D effect may clamp to its supported range.</value>
    /// <remarks>Mapped to D2D property index 0. Default: 0.0f.</remarks>
    public float InputWhiteLevel { get => (float)GetPropertyValue(InputWhiteLevelProperty)!; set => SetPropertyValue(InputWhiteLevelProperty, value); }

    /// <summary>
    /// Gets or sets the output white level value produced by the effect.
    /// </summary>
    /// <value>A floating-point value. The underlying D2D effect may clamp to its supported range.</value>
    /// <remarks>Mapped to D2D property index 1. Default: 0.0f.</remarks>
    public float OutputWhiteLevel { get => (float)GetPropertyValue(OutputWhiteLevelProperty)!; set => SetPropertyValue(OutputWhiteLevelProperty, value); }
}
