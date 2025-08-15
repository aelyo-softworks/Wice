namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1CompositeString)]
#else
[Guid(Constants.CLSID_D2D1CompositeString)]
#endif
/// <summary>
/// Represents the Direct2D/Win2D "Composite" effect, which combines multiple input sources
/// using a specified <see cref="D2D1_COMPOSITE_MODE"/> (e.g., SourceOver, Multiply, etc.).
/// </summary>
/// <remarks>
/// - The effect is exposed to D2D via <see cref="Effect"/> and <see cref="IGraphicsEffectD2D1Interop"/>.
/// - This effect accepts an unbounded number of sources (limited only by the graph setup),
///   as indicated by the constructor passing <c>int.MaxValue</c> to the base type.
/// - The CLSID is provided via <see cref="GuidAttribute"/> and switches between framework and .NET targets.
/// </remarks>
public partial class CompositeEffect : Effect
{
    /// <summary>
    /// Describes the effect property that controls the compositing mode.
    /// </summary>
    /// <remarks>
    /// - Index: <c>0</c> (first effect parameter in the D2D property bag).<br/>
    /// - Mapping: DIRECT (default mapping for effect properties).<br/>
    /// - Default: <see cref="D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER"/>.<br/>
    /// Use <see cref="Mode"/> to get/set the current value.
    /// </remarks>
    public static EffectProperty ModeProperty { get; }

    static CompositeEffect()
    {
        // Property 0 -> Mode (DIRECT mapping, default SourceOver)
        ModeProperty = EffectProperty.Add(typeof(CompositeEffect), nameof(Mode), 0, D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CompositeEffect"/> that may accept an unbounded number of sources.
    /// </summary>
    /// <remarks>
    /// Passing <c>int.MaxValue</c> to the base constructor enables dynamic source counts
    /// when interacting with the effect graph.
    /// </remarks>
    public CompositeEffect()
            : base(int.MaxValue)
    {
    }

    /// <summary>
    /// Gets or sets the compositing mode used to blend input sources.
    /// </summary>
    /// <value>
    /// A value from <see cref="D2D1_COMPOSITE_MODE"/>. Defaults to
    /// <see cref="D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER"/>.
    /// </value>
    public D2D1_COMPOSITE_MODE Mode
    {
        get => (D2D1_COMPOSITE_MODE)GetPropertyValue(ModeProperty)!;
        set => SetPropertyValue(ModeProperty, value);
    }
}
