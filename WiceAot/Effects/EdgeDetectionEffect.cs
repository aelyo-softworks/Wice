namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1EdgeDetectionString)]
public partial class EdgeDetectionEffect : EffectWithSource
{
    public static EffectProperty StrengthProperty { get; }
    public static EffectProperty BlurRadiusProperty { get; }
    public static EffectProperty ModeProperty { get; }
    public static EffectProperty OverlayEdgesProperty { get; }
    public static EffectProperty AlphaModeProperty { get; }

    static EdgeDetectionEffect()
    {
        StrengthProperty = EffectProperty.Add(typeof(EdgeDetectionEffect), nameof(Strength), 0, 0.5f);
        BlurRadiusProperty = EffectProperty.Add(typeof(EdgeDetectionEffect), nameof(BlurRadius), 1, 0f);
        ModeProperty = EffectProperty.Add(typeof(EdgeDetectionEffect), nameof(Mode), 2, D2D1_EDGEDETECTION_MODE.D2D1_EDGEDETECTION_MODE_SOBEL);
        OverlayEdgesProperty = EffectProperty.Add(typeof(EdgeDetectionEffect), nameof(OverlayEdges), 3, false);
        AlphaModeProperty = EffectProperty.Add(typeof(EdgeDetectionEffect), nameof(AlphaMode), 4, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLORMATRIX_ALPHA_MODE, D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED);
    }

    public float Strength { get => (float)GetPropertyValue(StrengthProperty)!; set => SetPropertyValue(StrengthProperty, value.Clamp(0.001f, 1f)); }
    public float BlurRadius { get => (float)GetPropertyValue(BlurRadiusProperty)!; set => SetPropertyValue(BlurRadiusProperty, value.Clamp(0f, 10f)); }
    public D2D1_EDGEDETECTION_MODE Mode { get => (D2D1_EDGEDETECTION_MODE)GetPropertyValue(ModeProperty)!; set => SetPropertyValue(ModeProperty, value); }
    public bool OverlayEdges { get => (bool)GetPropertyValue(OverlayEdgesProperty)!; set => SetPropertyValue(OverlayEdgesProperty, value); }
    public D2D1_ALPHA_MODE AlphaMode { get => (D2D1_ALPHA_MODE)GetPropertyValue(AlphaModeProperty)!; set => SetPropertyValue(AlphaModeProperty, value); }
}
