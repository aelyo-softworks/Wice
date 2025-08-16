namespace Wice.Effects;

/// <summary>
/// Represents an effect that adjusts the exposure level of an image.
/// </summary>
/// <remarks>The <see cref="ExposureEffect"/> allows you to modify the brightness of an image by adjusting its
/// exposure value. The exposure value can be set within a specific range to achieve the desired effect.</remarks>
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
    /// <remarks>This static constructor sets up the <see cref="ExposureValueProperty"/> by registering it as
    /// an effect property with a default value of 0.0f.</remarks>
    static ExposureEffect()
    {
        ExposureValueProperty = EffectProperty.Add(typeof(ExposureEffect), nameof(ExposureValue), 0, 0f);
    }

    /// <summary>
    /// Gets or sets the exposure value, which adjusts the brightness of an image.
    /// </summary>
    public float ExposureValue { get => (float)GetPropertyValue(ExposureValueProperty)!; set => SetPropertyValue(ExposureValueProperty, value.Clamp(-2f, 2f)); }
}
