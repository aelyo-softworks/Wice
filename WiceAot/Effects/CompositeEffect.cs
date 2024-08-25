namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1CompositeString)]
public partial class CompositeEffect : Effect
{
    public static EffectProperty ModeProperty { get; }

    static CompositeEffect()
    {
        ModeProperty = EffectProperty.Add(typeof(CompositeEffect), nameof(Mode), 0, D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER);
    }

    public CompositeEffect()
            : base(int.MaxValue)
    {
    }

    public D2D1_COMPOSITE_MODE Mode { get => (D2D1_COMPOSITE_MODE)GetPropertyValue(ModeProperty)!; set => SetPropertyValue(ModeProperty, value); }
}
