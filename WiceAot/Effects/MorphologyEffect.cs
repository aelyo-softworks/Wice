namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1MorphologyString)]
public class MorphologyEffect : EffectWithSource
{
    public static EffectProperty ModeProperty { get; }
    public static EffectProperty WidthProperty { get; }
    public static EffectProperty HeightProperty { get; }

    static MorphologyEffect()
    {
        ModeProperty = EffectProperty.Add(typeof(MorphologyEffect), nameof(Mode), 0, D2D1_MORPHOLOGY_MODE.D2D1_MORPHOLOGY_MODE_ERODE);
        WidthProperty = EffectProperty.Add(typeof(MorphologyEffect), nameof(Width), 1, 1u);
        HeightProperty = EffectProperty.Add(typeof(MorphologyEffect), nameof(Height), 2, 1u);
    }

    public D2D1_MORPHOLOGY_MODE Mode { get => (D2D1_MORPHOLOGY_MODE)GetPropertyValue(ModeProperty); set => SetPropertyValue(ModeProperty, value); }
    public uint Width { get => (uint)GetPropertyValue(WidthProperty); set => SetPropertyValue(WidthProperty, value.Clamp(1, 100)); }
    public uint Height { get => (uint)GetPropertyValue(HeightProperty); set => SetPropertyValue(HeightProperty, value.Clamp(1, 100)); }
}
