namespace Wice.Effects;

/// <summary>
/// Wraps the built-in Direct2D Brightness effect (CLSID_D2D1Brightness) for use in effect graphs.
/// </summary>
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
    public static EffectProperty WhitePointProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="BlackPoint"/>.
    /// </summary>
    public static EffectProperty BlackPointProperty { get; }

    static BrightnessEffect()
    {
        WhitePointProperty = EffectProperty.Add(typeof(BrightnessEffect), nameof(WhitePoint), 0, new D2D_VECTOR_2F(1f, 1f));
        BlackPointProperty = EffectProperty.Add(typeof(BrightnessEffect), nameof(BlackPoint), 1, new D2D_VECTOR_2F());
    }

    /// <summary>
    /// Gets or sets the upper luminance bound used by the brightness remap.
    /// </summary>
    public D2D_VECTOR_2F WhitePoint
    {
        get => (D2D_VECTOR_2F)GetPropertyValue(WhitePointProperty)!;
        set => SetPropertyValue(WhitePointProperty, value);
    }

    /// <summary>
    /// Gets or sets the lower luminance bound used by the brightness remap.
    /// </summary>
    public D2D_VECTOR_2F BlackPoint
    {
        get => (D2D_VECTOR_2F)GetPropertyValue(BlackPointProperty)!;
        set => SetPropertyValue(BlackPointProperty, value);
    }
}
