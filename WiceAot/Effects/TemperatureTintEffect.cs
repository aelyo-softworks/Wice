namespace Wice.Effects;

/// <summary>
/// Direct2D Temperature and Tint effect wrapper.
/// </summary>
/// <remarks>
/// - Exposes two parameters mapped by index:
///   0 → <see cref="Temperature"/>, 1 → <see cref="Tint"/>.
/// - Both parameters are normalized to the [-1, +1] range and are clamped on assignment.
/// - The effect requires a source (<see cref="EffectWithSource.Source"/>).
/// </remarks>
/// <seealso cref="EffectWithSource"/>
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
    /// <remarks>
    /// - Index: 0 (D2D property bag binding)
    /// - Type: <see cref="float"/>
    /// - Default: 0f
    /// - Mapping: DIRECT
    /// </remarks>
    public static EffectProperty TemperatureProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="Tint"/>.
    /// </summary>
    /// <remarks>
    /// - Index: 1 (D2D property bag binding)
    /// - Type: <see cref="float"/>
    /// - Default: 0f
    /// - Mapping: DIRECT
    /// </remarks>
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
    /// <remarks>
    /// - Range: [-1, +1] (values are clamped)
    /// - 0: neutral (no change)
    /// - Negative values: cooler
    /// - Positive values: warmer
    /// </remarks>
    public float Temperature
    {
        get => (float)GetPropertyValue(TemperatureProperty)!;
        set => SetPropertyValue(TemperatureProperty, value.Clamp(-1f, 1f));
    }

    /// <summary>
    /// Adjusts the green/magenta tint of the image.
    /// </summary>
    /// <remarks>
    /// - Range: [-1, +1] (values are clamped)
    /// - 0: neutral (no change)
    /// - Negative values: greener
    /// - Positive values: magenta
    /// </remarks>
    public float Tint
    {
        get => (float)GetPropertyValue(TintProperty)!;
        set => SetPropertyValue(TintProperty, value.Clamp(-1f, 1f));
    }
}
