namespace Wice.Effects;

[Guid(Constants.CLSID_D2D1BorderString)]
public partial class BorderEffect : EffectWithSource
{
    public static EffectProperty EdgeModeXProperty { get; }
    public static EffectProperty EdgeModeYProperty { get; }

    static BorderEffect()
    {
        EdgeModeXProperty = EffectProperty.Add(typeof(BorderEffect), nameof(EdgeModeX), 0, D2D1_BORDER_EDGE_MODE.D2D1_BORDER_EDGE_MODE_CLAMP);
        EdgeModeYProperty = EffectProperty.Add(typeof(BorderEffect), nameof(EdgeModeY), 1, D2D1_BORDER_EDGE_MODE.D2D1_BORDER_EDGE_MODE_CLAMP);
    }

    public D2D1_BORDER_EDGE_MODE EdgeModeX { get => (D2D1_BORDER_EDGE_MODE)GetPropertyValue(EdgeModeXProperty)!; set => SetPropertyValue(EdgeModeXProperty, value); }
    public D2D1_BORDER_EDGE_MODE EdgeModeY { get => (D2D1_BORDER_EDGE_MODE)GetPropertyValue(EdgeModeYProperty)!; set => SetPropertyValue(EdgeModeYProperty, value); }
}
