namespace Wice.Effects;

/// <summary>
/// Represents an effect that adjusts the highlights and shadows of an image, with optional clarity enhancement and
/// gamma correction.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1HighlightsShadowsString)]
#else
[Guid(Constants.CLSID_D2D1HighlightsShadowsString)]
#endif
public partial class HighlightsShadowsEffect : EffectWithSource
{
    /// <summary>
    /// Gets the property that represents the highlights effect in an image processing pipeline.
    /// </summary>
    public static EffectProperty HighlightsProperty { get; }

    /// <summary>
    /// Gets the property that represents shadow effects in the rendering system.
    /// </summary>
    public static EffectProperty ShadowsProperty { get; }

    /// <summary>
    /// Gets the property that represents the clarity effect configuration.
    /// </summary>
    public static EffectProperty ClarityProperty { get; }

    /// <summary>
    /// Gets the effect property that specifies the input gamma value for the effect.
    /// </summary>
    public static EffectProperty InputGammaProperty { get; }

    /// <summary>
    /// Gets the dependency property that represents the blur radius applied to the mask effect.
    /// </summary>
    public static EffectProperty MaskBlurRadiusProperty { get; }

    static HighlightsShadowsEffect()
    {
        HighlightsProperty = EffectProperty.Add(typeof(HighlightsShadowsEffect), nameof(Highlights), 0, 0f);
        ShadowsProperty = EffectProperty.Add(typeof(HighlightsShadowsEffect), nameof(Shadows), 1, 0f);
        ClarityProperty = EffectProperty.Add(typeof(HighlightsShadowsEffect), nameof(Clarity), 2, 0f);
        InputGammaProperty = EffectProperty.Add(typeof(HighlightsShadowsEffect), nameof(InputGamma), 3, D2D1_HIGHLIGHTSANDSHADOWS_INPUT_GAMMA.D2D1_HIGHLIGHTSANDSHADOWS_INPUT_GAMMA_LINEAR);
        MaskBlurRadiusProperty = EffectProperty.Add(typeof(HighlightsShadowsEffect), nameof(MaskBlurRadius), 4, 1.25f);
    }

    /// <summary>
    /// Gets or sets the highlights adjustment value.
    /// </summary>
    public float Highlights { get => (float)GetPropertyValue(HighlightsProperty)!; set => SetPropertyValue(HighlightsProperty, value.Clamp(-1f, 1f)); }

    /// <summary>
    /// Gets or sets the shadow intensity for the object.
    /// </summary>
    public float Shadows { get => (float)GetPropertyValue(ShadowsProperty)!; set => SetPropertyValue(ShadowsProperty, value.Clamp(-1f, 1f)); }

    /// <summary>
    /// Gets or sets the clarity level of the object.
    /// </summary>
    public float Clarity { get => (float)GetPropertyValue(ClarityProperty)!; set => SetPropertyValue(ClarityProperty, value.Clamp(-1f, 1f)); }

    /// <summary>
    /// Gets or sets the input gamma value used for processing highlights and shadows.
    /// </summary>
    public D2D1_HIGHLIGHTSANDSHADOWS_INPUT_GAMMA InputGamma { get => (D2D1_HIGHLIGHTSANDSHADOWS_INPUT_GAMMA)GetPropertyValue(InputGammaProperty)!; set => SetPropertyValue(InputGammaProperty, value); }

    /// <summary>
    /// Gets or sets the blur radius applied to the mask. 
    /// </summary>
    public float MaskBlurRadius { get => (float)GetPropertyValue(MaskBlurRadiusProperty)!; set => SetPropertyValue(MaskBlurRadiusProperty, value.Clamp(0f, 10f)); }
}
