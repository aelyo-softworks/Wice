namespace Wice.Effects;

/// <summary>
/// Direct2D Border effect wrapper.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1BorderString)]
#else
[Guid(Constants.CLSID_D2D1BorderString)]
#endif
public partial class BorderEffect : EffectWithSource
{
    /// <summary>
    /// Effect property descriptor for <see cref="EdgeModeX"/>.
    /// </summary>
    public static EffectProperty EdgeModeXProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="EdgeModeY"/>.
    /// </summary>
    public static EffectProperty EdgeModeYProperty { get; }

    // Initializes effect property descriptors for parameter indices 0 (X) and 1 (Y).
    static BorderEffect()
    {
        EdgeModeXProperty = EffectProperty.Add(typeof(BorderEffect), nameof(EdgeModeX), 0, D2D1_BORDER_EDGE_MODE.D2D1_BORDER_EDGE_MODE_CLAMP);
        EdgeModeYProperty = EffectProperty.Add(typeof(BorderEffect), nameof(EdgeModeY), 1, D2D1_BORDER_EDGE_MODE.D2D1_BORDER_EDGE_MODE_CLAMP);
    }

    /// <summary>
    /// Gets or sets the edge handling mode along the X axis when sampling outside the input bounds.
    /// </summary>
    public D2D1_BORDER_EDGE_MODE EdgeModeX
    {
        get => (D2D1_BORDER_EDGE_MODE)GetPropertyValue(EdgeModeXProperty)!;
        set => SetPropertyValue(EdgeModeXProperty, value);
    }

    /// <summary>
    /// Gets or sets the edge handling mode along the Y axis when sampling outside the input bounds.
    /// </summary>
    public D2D1_BORDER_EDGE_MODE EdgeModeY
    {
        get => (D2D1_BORDER_EDGE_MODE)GetPropertyValue(EdgeModeYProperty)!;
        set => SetPropertyValue(EdgeModeYProperty, value);
    }
}
