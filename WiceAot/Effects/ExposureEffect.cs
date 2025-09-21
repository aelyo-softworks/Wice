namespace Wice.Effects;

/// <summary>
/// Represents an effect that adjusts the exposure level of an image.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1ExposureString)]
#else
[Guid(Constants.CLSID_D2D1ExposureString)]
#endif
public partial class ExposureEffect : EffectWithSource
{
    /// <summary>
    /// Gets the static property that represents the exposure value for an effect.
    /// </summary>
    public static EffectProperty ExposureValueProperty { get; }

    /// <summary>
    /// Initializes static members of the <see cref="ExposureEffect"/> class.
    /// </summary>
    static ExposureEffect()
    {
        ExposureValueProperty = EffectProperty.Add(typeof(ExposureEffect), nameof(ExposureValue), 0, 0f);
    }

    /// <summary>
    /// Gets or sets the exposure value, which adjusts the brightness of an image.
    /// </summary>
    public float ExposureValue { get => (float)GetPropertyValue(ExposureValueProperty)!; set => SetPropertyValue(ExposureValueProperty, value.Clamp(-2f, 2f)); }
}
