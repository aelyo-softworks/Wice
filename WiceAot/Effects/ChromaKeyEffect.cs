namespace Wice.Effects;

/// <summary>
/// Direct2D chroma key (green-screen) effect.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1ChromaKeyString)]
#else
[Guid(Constants.CLSID_D2D1ChromaKeyString)]
#endif
public partial class ChromaKeyEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for <see cref="Color"/> (effect property index 0, mapped as COLOR_TO_VECTOR3).
    /// </summary>
    public static EffectProperty ColorProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="Tolerance"/> (effect property index 1).
    /// </summary>
    public static EffectProperty ToleranceProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="InvertAlpha"/> (effect property index 2).
    /// </summary>
    public static EffectProperty InvertAlphaProperty { get; }

    /// <summary>
    /// Descriptor for <see cref="Feather"/> (effect property index 3).
    /// </summary>
    public static EffectProperty FeatherProperty { get; }

    static ChromaKeyEffect()
    {
        ColorProperty = EffectProperty.Add(typeof(ChromaKeyEffect), nameof(Color), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3, new D2D_VECTOR_3F());
        ToleranceProperty = EffectProperty.Add(typeof(ChromaKeyEffect), nameof(Tolerance), 1, 0.1f);
        InvertAlphaProperty = EffectProperty.Add(typeof(ChromaKeyEffect), nameof(InvertAlpha), 2, false);
        FeatherProperty = EffectProperty.Add(typeof(ChromaKeyEffect), nameof(Feather), 3, false);
    }

    /// <summary>
    /// Key color to remove from the source image.
    /// </summary>
    public D2D_VECTOR_3F Color { get => (D2D_VECTOR_3F)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }

    /// <summary>
    /// Color match tolerance in [0, 1]. Higher values select a wider range around <see cref="Color"/>.
    /// </summary>
    public float Tolerance { get => (float)GetPropertyValue(ToleranceProperty)!; set => SetPropertyValue(ToleranceProperty, value.Clamp(0f, 1f)); }

    /// <summary>
    /// Inverts the computed alpha mask.
    /// </summary>
    public bool InvertAlpha { get => (bool)GetPropertyValue(InvertAlphaProperty)!; set => SetPropertyValue(InvertAlphaProperty, value); }

    /// <summary>
    /// Enables soft edge feathering on the transparency boundary.
    /// </summary>
    public bool Feather { get => (bool)GetPropertyValue(FeatherProperty)!; set => SetPropertyValue(FeatherProperty, value); }
}
