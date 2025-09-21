namespace Wice.Effects;

/// <summary>
/// Wraps the Direct2D WhiteLevelAdjustment effect (CLSID_D2D1WhiteLevelAdjustment).
/// </summary>
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
    public static EffectProperty InputWhiteLevelProperty { get; }

    /// <summary>
    /// Descriptor for the <see cref="OutputWhiteLevel"/> parameter.
    /// </summary>
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
    public float InputWhiteLevel { get => (float)GetPropertyValue(InputWhiteLevelProperty)!; set => SetPropertyValue(InputWhiteLevelProperty, value); }

    /// <summary>
    /// Gets or sets the output white level value produced by the effect.
    /// </summary>
    /// <value>A floating-point value. The underlying D2D effect may clamp to its supported range.</value>
    public float OutputWhiteLevel { get => (float)GetPropertyValue(OutputWhiteLevelProperty)!; set => SetPropertyValue(OutputWhiteLevelProperty, value); }
}
