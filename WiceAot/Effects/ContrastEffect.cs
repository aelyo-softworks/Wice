namespace Wice.Effects;

/// <summary>
/// Represents an effect that adjusts the contrast of an input image.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1ContrastString)]
#else
[Guid(Constants.CLSID_D2D1ContrastString)]
#endif
public partial class ContrastEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for the <see cref="Contrast"/> effect parameter.
    /// </summary>
    public static EffectProperty ContrastProperty { get; }

    /// <summary>
    /// Descriptor for the <see cref="ClampInput"/> effect parameter.
    /// </summary>
    public static EffectProperty ClampInputProperty { get; }

    static ContrastEffect()
    {
        // Register effect properties with their indices and defaults.
        ContrastProperty = EffectProperty.Add(typeof(ContrastEffect), nameof(Contrast), 0, 0f);
        ClampInputProperty = EffectProperty.Add(typeof(ContrastEffect), nameof(ClampInput), 1, false);
    }

    /// <summary>
    /// Gets or sets the contrast adjustment amount.
    /// </summary>
    /// <value>
    /// A value in the range [-1.0, 1.0], where 0 means no change, positive values increase contrast,
    /// and negative values decrease contrast.
    /// </value>
    public float Contrast
    {
        get => (float)GetPropertyValue(ContrastProperty)!;
        set => SetPropertyValue(ContrastProperty, value.Clamp(-1f, 1f));
    }

    /// <summary>
    /// Gets or sets a value indicating whether the input is clamped to the [0, 1] range before contrast is applied.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to clamp the input; otherwise, <see langword="false"/>. Default is <see langword="false"/>.
    /// </value>
    public bool ClampInput
    {
        get => (bool)GetPropertyValue(ClampInputProperty)!;
        set => SetPropertyValue(ClampInputProperty, value);
    }
}
