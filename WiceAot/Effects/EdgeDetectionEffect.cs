namespace Wice.Effects;

/// <summary>
/// Represents an edge detection effect that identifies and highlights edges in an image.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1EdgeDetectionString)]
#else
[Guid(Constants.CLSID_D2D1EdgeDetectionString)]
#endif
public partial class EdgeDetectionEffect : EffectWithSource
{
    /// <summary>
    /// Gets the effect property representing the strength of the effect.
    /// </summary>
    public static EffectProperty StrengthProperty { get; }

    /// <summary>
    /// Gets the dependency property that identifies the blur radius of an effect.
    /// </summary>
    public static EffectProperty BlurRadiusProperty { get; }

    /// <summary>
    /// Gets the dependency property that identifies the mode of the effect.
    /// </summary>
    public static EffectProperty ModeProperty { get; }

    /// <summary>
    /// Gets the static property that represents the overlay edges effect configuration.
    /// </summary>
    public static EffectProperty OverlayEdgesProperty { get; }

    /// <summary>
    /// Gets the static property that represents the alpha mode configuration for an effect.
    /// </summary>
    public static EffectProperty AlphaModeProperty { get; }

    static EdgeDetectionEffect()
    {
        StrengthProperty = EffectProperty.Add(typeof(EdgeDetectionEffect), nameof(Strength), 0, 0.5f);
        BlurRadiusProperty = EffectProperty.Add(typeof(EdgeDetectionEffect), nameof(BlurRadius), 1, 0f);
        ModeProperty = EffectProperty.Add(typeof(EdgeDetectionEffect), nameof(Mode), 2, D2D1_EDGEDETECTION_MODE.D2D1_EDGEDETECTION_MODE_SOBEL);
        OverlayEdgesProperty = EffectProperty.Add(typeof(EdgeDetectionEffect), nameof(OverlayEdges), 3, false);
        AlphaModeProperty = EffectProperty.Add(typeof(EdgeDetectionEffect), nameof(AlphaMode), 4, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLORMATRIX_ALPHA_MODE, D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED);
    }

    /// <summary>
    /// Gets or sets the strength value, constrained between 0.001 and 1.0.
    /// </summary>
    public float Strength { get => (float)GetPropertyValue(StrengthProperty)!; set => SetPropertyValue(StrengthProperty, value.Clamp(0.001f, 1f)); }

    /// <summary>
    /// Gets or sets the radius of the blur effect applied to the element.
    /// </summary>
    public float BlurRadius { get => (float)GetPropertyValue(BlurRadiusProperty)!; set => SetPropertyValue(BlurRadiusProperty, value.Clamp(0f, 10f)); }

    /// <summary>
    /// Gets or sets the edge detection mode used by the algorithm.
    /// </summary>
    public D2D1_EDGEDETECTION_MODE Mode { get => (D2D1_EDGEDETECTION_MODE)GetPropertyValue(ModeProperty)!; set => SetPropertyValue(ModeProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether edges should be overlaid in the current rendering context.
    /// </summary>
    public bool OverlayEdges { get => (bool)GetPropertyValue(OverlayEdgesProperty)!; set => SetPropertyValue(OverlayEdgesProperty, value); }

    /// <summary>
    /// Gets or sets the alpha mode used to interpret the transparency of the content.
    /// </summary>
    public D2D1_ALPHA_MODE AlphaMode { get => (D2D1_ALPHA_MODE)GetPropertyValue(AlphaModeProperty)!; set => SetPropertyValue(AlphaModeProperty, value); }
}
