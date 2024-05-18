namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1RgbToHueString)]
public class RgbToHueEffect : EffectWithSource
{
    public static EffectProperty OutputColorSpaceProperty { get; }

    static RgbToHueEffect()
    {
        OutputColorSpaceProperty = EffectProperty.Add(typeof(RgbToHueEffect), nameof(OutputColorSpace), 0, D2D1_RGBTOHUE_OUTPUT_COLOR_SPACE.D2D1_RGBTOHUE_OUTPUT_COLOR_SPACE_HUE_SATURATION_VALUE);
    }

    public D2D1_RGBTOHUE_OUTPUT_COLOR_SPACE OutputColorSpace { get => (D2D1_RGBTOHUE_OUTPUT_COLOR_SPACE)GetPropertyValue(OutputColorSpaceProperty); set => SetPropertyValue(OutputColorSpaceProperty, value); }
}
