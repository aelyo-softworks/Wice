namespace Wice.Effects;

/// <summary>
/// Represents a 3D lookup table (LUT) effect that applies color transformations to an image source.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1LookupTable3DString)]
#else
[Guid(Constants.CLSID_D2D1LookupTable3DString)]
#endif
public partial class LookupTable3DEffect : EffectWithSource
{
    /// <summary>
    /// Gets the effect property representing the lookup table (LUT) used for color grading.
    /// </summary>
    public static EffectProperty LutProperty { get; }

    /// <summary>
    /// Gets the static property that represents the alpha mode configuration for an effect.
    /// </summary>
    public static EffectProperty AlphaModeProperty { get; }

    /// <summary>
    /// Initializes static properties for the <see cref="LookupTable3DEffect"/> class.
    /// </summary>
    static LookupTable3DEffect()
    {
        LutProperty = EffectProperty.Add<object>(typeof(LookupTable3DEffect), nameof(Lut), 0);
        AlphaModeProperty = EffectProperty.Add(typeof(LookupTable3DEffect), nameof(AlphaMode), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLORMATRIX_ALPHA_MODE, D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED);
    }

    /// <summary>
    /// Gets or sets the lookup table (LUT) associated with this object.
    /// </summary>
    public object Lut { get => GetPropertyValue(LutProperty)!; set => SetPropertyValue(LutProperty, value); }

    /// <summary>
    /// Gets or sets the alpha mode used for rendering operations.
    /// </summary>
    public D2D1_ALPHA_MODE AlphaMode { get => (D2D1_ALPHA_MODE)GetPropertyValue(AlphaModeProperty)!; set => SetPropertyValue(AlphaModeProperty, value); }
}
