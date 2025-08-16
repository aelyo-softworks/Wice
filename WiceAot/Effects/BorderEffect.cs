namespace Wice.Effects;

/// <summary>
/// Direct2D Border effect wrapper.
/// </summary>
/// <remarks>
/// - Controls how sampling outside the input bounds is handled along X and Y axes.
/// - Exposes two enum properties (<see cref="EdgeModeX"/>, <see cref="EdgeModeY"/>) of type <see cref="D2D1_BORDER_EDGE_MODE"/>.
/// - Parameter indices follow D2D: 0 = X edge mode, 1 = Y edge mode.
/// - Inherits from <see cref="EffectWithSource"/> and requires a primary source.
/// </remarks>
/// <seealso cref="EffectWithSource"/>
/// <seealso cref="D2D1_BORDER_EDGE_MODE"/>
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
    /// <remarks>
    /// - Index: 0
    /// - Type: <see cref="D2D1_BORDER_EDGE_MODE"/>
    /// - Default: <see cref="D2D1_BORDER_EDGE_MODE.D2D1_BORDER_EDGE_MODE_CLAMP"/>
    /// </remarks>
    public static EffectProperty EdgeModeXProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="EdgeModeY"/>.
    /// </summary>
    /// <remarks>
    /// - Index: 1
    /// - Type: <see cref="D2D1_BORDER_EDGE_MODE"/>
    /// - Default: <see cref="D2D1_BORDER_EDGE_MODE.D2D1_BORDER_EDGE_MODE_CLAMP"/>
    /// </remarks>
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
    /// <remarks>
    /// Maps to Direct2D Border effect parameter at index 0.
    /// </remarks>
    public D2D1_BORDER_EDGE_MODE EdgeModeX
    {
        get => (D2D1_BORDER_EDGE_MODE)GetPropertyValue(EdgeModeXProperty)!;
        set => SetPropertyValue(EdgeModeXProperty, value);
    }

    /// <summary>
    /// Gets or sets the edge handling mode along the Y axis when sampling outside the input bounds.
    /// </summary>
    /// <remarks>
    /// Maps to Direct2D Border effect parameter at index 1.
    /// </remarks>
    public D2D1_BORDER_EDGE_MODE EdgeModeY
    {
        get => (D2D1_BORDER_EDGE_MODE)GetPropertyValue(EdgeModeYProperty)!;
        set => SetPropertyValue(EdgeModeYProperty, value);
    }
}
