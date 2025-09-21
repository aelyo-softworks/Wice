namespace Wice.Effects;

/// <summary>
/// Direct2D Morphology effect wrapper.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1MorphologyString)]
#else
[Guid(Constants.CLSID_D2D1MorphologyString)]
#endif
public partial class MorphologyEffect : EffectWithSource
{
    /// <summary>
    /// Gets the effect property descriptor for <see cref="Mode"/>.
    /// </summary>
    public static EffectProperty ModeProperty { get; }

    /// <summary>
    /// Gets the effect property descriptor for <see cref="Width"/>.
    /// </summary>
    public static EffectProperty WidthProperty { get; }

    /// <summary>
    /// Gets the effect property descriptor for <see cref="Height"/>.
    /// </summary>
    public static EffectProperty HeightProperty { get; }

    /// <summary>
    /// Registers effect properties and their D2D indices/defaults.
    /// </summary>
    static MorphologyEffect()
    {
        ModeProperty = EffectProperty.Add(typeof(MorphologyEffect), nameof(Mode), 0, D2D1_MORPHOLOGY_MODE.D2D1_MORPHOLOGY_MODE_ERODE);
        WidthProperty = EffectProperty.Add(typeof(MorphologyEffect), nameof(Width), 1, 1u);
        HeightProperty = EffectProperty.Add(typeof(MorphologyEffect), nameof(Height), 2, 1u);
    }

    /// <summary>
    /// Gets or sets the morphology operation to apply.
    /// </summary>
    public D2D1_MORPHOLOGY_MODE Mode { get => (D2D1_MORPHOLOGY_MODE)GetPropertyValue(ModeProperty)!; set => SetPropertyValue(ModeProperty, value); }

    /// <summary>
    /// Gets or sets the horizontal size of the structuring element in pixels.
    /// </summary>
    /// <value>
    /// Clamped to [1, 100]. Default is 1.
    /// </value>
    public uint Width { get => (uint)GetPropertyValue(WidthProperty)!; set => SetPropertyValue(WidthProperty, value.Clamp(1, 100)); }

    /// <summary>
    /// Gets or sets the vertical size of the structuring element in pixels.
    /// </summary>
    /// <value>
    /// Clamped to [1, 100]. Default is 1.
    /// </value>
    public uint Height { get => (uint)GetPropertyValue(HeightProperty)!; set => SetPropertyValue(HeightProperty, value.Clamp(1, 100)); }
}
