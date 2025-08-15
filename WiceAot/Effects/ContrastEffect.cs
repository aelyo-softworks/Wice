namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1ContrastString)]
#else
[Guid(Constants.CLSID_D2D1ContrastString)]
#endif
/// <summary>
/// Wraps the Direct2D/Win2D Contrast effect, exposing its parameters as CLR properties.
/// </summary>
/// <remarks>
/// - The effect requires at least one input source (see <see cref="EffectWithSource"/> and <see cref="Source"/>).
/// - The class is attributed with the effect CLSID for D2D interop using <see cref="GuidAttribute"/> (conditional for .NET Framework).
/// - Effect properties are registered through <see cref="EffectProperty"/> and mapped by index for D2D queries.
/// </remarks>
/// <seealso cref="EffectWithSource"/>
public partial class ContrastEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for the <see cref="Contrast"/> effect parameter.
    /// </summary>
    /// <remarks>
    /// - Index: 0 (GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT).<br/>
    /// - Type: <see cref="float"/>.<br/>
    /// - Default: 0.0f (no contrast adjustment).
    /// </remarks>
    public static EffectProperty ContrastProperty { get; }

    /// <summary>
    /// Descriptor for the <see cref="ClampInput"/> effect parameter.
    /// </summary>
    /// <remarks>
    /// - Index: 1 (GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT).<br/>
    /// - Type: <see cref="bool"/>.<br/>
    /// - Default: <see langword="false"/>.
    /// </remarks>
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
    /// <remarks>
    /// The value is clamped to [-1.0, 1.0] on assignment. This parameter is exposed at index 0.
    /// </remarks>
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
    /// <remarks>
    /// This parameter is exposed at index 1.
    /// </remarks>
    public bool ClampInput
    {
        get => (bool)GetPropertyValue(ClampInputProperty)!;
        set => SetPropertyValue(ClampInputProperty, value);
    }
}
