namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1VignetteString)]
public partial class VignetteEffect : EffectWithSource
{
    public static EffectProperty ColorProperty { get; }
    public static EffectProperty TransitionSizeProperty { get; }
    public static EffectProperty StrengthProperty { get; }

    static VignetteEffect()
    {
        ColorProperty = EffectProperty.Add(typeof(VignetteEffect), nameof(Color), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR4, D3DCOLORVALUE.Black);
        TransitionSizeProperty = EffectProperty.Add(typeof(VignetteEffect), nameof(TransitionSize), 1, 0f);
        StrengthProperty = EffectProperty.Add(typeof(VignetteEffect), nameof(Strength), 2, 0f);
    }

    public D3DCOLORVALUE Color { get => (D3DCOLORVALUE)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }
    public float TransitionSize { get => (float)GetPropertyValue(TransitionSizeProperty)!; set => SetPropertyValue(TransitionSizeProperty, value); }
    public float Strength { get => (float)GetPropertyValue(StrengthProperty)!; set => SetPropertyValue(StrengthProperty, value); }
}
