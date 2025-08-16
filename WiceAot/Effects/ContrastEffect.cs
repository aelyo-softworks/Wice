namespace Wice.Effects;

/// <summary>
/// Represents an effect that adjusts the contrast of an input image.
/// </summary>
/// <remarks>The <see cref="ContrastEffect"/> allows you to modify the contrast of an image by applying a contrast
/// adjustment factor. The effect provides two configurable parameters: <list type="bullet"> <item> <description><see
/// cref="Contrast"/>: Adjusts the contrast level. Positive values increase contrast, negative values decrease it, and 0
/// applies no adjustment.</description> </item> <item> <description><see cref="ClampInput"/>: Determines whether the
/// input is clamped to the [0, 1] range before applying the contrast adjustment.</description> </item> </list> This
/// effect is typically used in image processing pipelines to enhance or reduce the contrast of visual
/// content.</remarks>
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
