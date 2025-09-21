namespace Wice.Effects;

/// <summary>
/// Direct2D Sepia effect wrapper with a single input source.
/// Exposes two effect properties:
/// - <see cref="Intensity"/> (index 0): sepia intensity in [0,1], default 0.5.
/// - <see cref="AlphaMode"/> (index 1): alpha mode hint, default <see cref="D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED"/>.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1SepiaString)]
#else
[Guid(Constants.CLSID_D2D1SepiaString)]
#endif
public partial class SepiaEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for <see cref="Intensity"/>. Registered at effect parameter index 0.
    /// Default value: 0.5f.
    /// </summary>
    public static EffectProperty IntensityProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="AlphaMode"/>. Registered at effect parameter index 1 with mapping
    /// <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLORMATRIX_ALPHA_MODE"/>.
    /// Default value: <see cref="D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED"/>.
    /// </summary>
    public static EffectProperty AlphaModeProperty { get; }

    /// <summary>
    /// Registers effect property descriptors for this type.
    /// </summary>
    static SepiaEffect()
    {
        // Intensity (index 0), default 0.5f
        IntensityProperty = EffectProperty.Add(typeof(SepiaEffect), nameof(Intensity), 0, 0.5f);

        // AlphaMode (index 1), mapping hint = ColorMatrix alpha mode, default = Premultiplied
        AlphaModeProperty = EffectProperty.Add(
            typeof(SepiaEffect),
            nameof(AlphaMode),
            1,
            GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLORMATRIX_ALPHA_MODE,
            D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED);
    }

    /// <summary>
    /// Gets or sets the sepia intensity in the range [0, 1].
    /// Values are clamped and the default is 0.5.
    /// Maps to effect parameter index 0.
    /// </summary>
    public float Intensity
    {
        get => (float)GetPropertyValue(IntensityProperty)!;
        set => SetPropertyValue(IntensityProperty, value.Clamp(0f, 1f));
    }

    /// <summary>
    /// Gets or sets the alpha mode used by the effect.
    /// Default is <see cref="D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED"/>.
    /// Maps to effect parameter index 1 with mapping
    /// <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLORMATRIX_ALPHA_MODE"/>.
    /// </summary>
    public D2D1_ALPHA_MODE AlphaMode
    {
        get => (D2D1_ALPHA_MODE)GetPropertyValue(AlphaModeProperty)!;
        set => SetPropertyValue(AlphaModeProperty, value);
    }
}
