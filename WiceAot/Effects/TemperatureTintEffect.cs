namespace Wice.Effects;

/// <summary>
/// Direct2D Temperature and Tint effect wrapper.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1TemperatureTintString)]
#else
[Guid(Constants.CLSID_D2D1TemperatureTintString)]
#endif
public partial class TemperatureTintEffect : EffectWithSource
{
    /// <summary>
    /// Effect property descriptor for <see cref="Temperature"/>.
    /// </summary>
    public static EffectProperty TemperatureProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="Tint"/>.
    /// </summary>
    public static EffectProperty TintProperty { get; }

    /// <summary>
    /// Registers effect property descriptors and freezes their metadata.
    /// </summary>
    static TemperatureTintEffect()
    {
        TemperatureProperty = EffectProperty.Add(typeof(TemperatureTintEffect), nameof(Temperature), 0, 0f);
        TintProperty = EffectProperty.Add(typeof(TemperatureTintEffect), nameof(Tint), 1, 0f);
    }

    /// <summary>
    /// Adjusts the white balance temperature of the image.
    /// </summary>
    public float Temperature
    {
        get => (float)GetPropertyValue(TemperatureProperty)!;
        set => SetPropertyValue(TemperatureProperty, value.Clamp(-1f, 1f));
    }

    /// <summary>
    /// Adjusts the green/magenta tint of the image.
    /// </summary>
    public float Tint
    {
        get => (float)GetPropertyValue(TintProperty)!;
        set => SetPropertyValue(TintProperty, value.Clamp(-1f, 1f));
    }
}
