namespace Wice.Effects;

#if NETFRAMEWORK
/// <summary>
/// CLSID for the Direct2D Composite effect on .NET Framework.
/// </summary>
[Guid(D2D1Constants.CLSID_D2D1CompositeString)]
#else
/// <summary>
/// CLSID for the Direct2D Composite effect on .NET (Core/5+).
/// </summary>
[Guid(Constants.CLSID_D2D1CompositeString)]
#endif
/// <summary>
/// Direct2D/Win2D composite effect node that blends two input sources using a configurable mode.
/// </summary>
/// <remarks>
/// - This type maps to the native D2D1 Composite effect (CLSID_D2D1Composite).
/// - It exposes two input slots:
///   - <see cref="Destination"/> at index 0 (the "destination" layer),
///   - <see cref="Source"/> at index 1 (the "source" layer).
/// - The blend operation is controlled via <see cref="Mode"/>.
/// </remarks>
/// <seealso cref="Effect"/>
/// <seealso cref="IGraphicsEffectSource"/>
public partial class CompositeStepEffect : Effect
{
    /// <summary>
    /// Descriptor for the <see cref="Mode"/> effect parameter.
    /// </summary>
    /// <remarks>
    /// - Effect property index: 0 (serialized into the effect property bag at index 0).<br/>
    /// - CLR type: <see cref="D2D1_COMPOSITE_MODE"/>.<br/>
    /// - Default value: <see cref="D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER"/>.<br/>
    /// - Mapping: direct (no special transformation).
    /// </remarks>
    public static EffectProperty ModeProperty { get; }

    // Registers the Mode property descriptor on first use.
    static CompositeStepEffect()
    {
        ModeProperty = EffectProperty.Add(typeof(CompositeStepEffect), nameof(Mode), 0, D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER);
    }

    /// <summary>
    /// Initializes a new instance with two input slots (destination at index 0, source at index 1).
    /// </summary>
    public CompositeStepEffect()
            : base(2)
    {
    }

    /// <summary>
    /// Gets or sets the composite operation used to blend <see cref="Source"/> over <see cref="Destination"/>.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER"/>.
    /// </remarks>
    public D2D1_COMPOSITE_MODE Mode { get => (D2D1_COMPOSITE_MODE)GetPropertyValue(ModeProperty)!; set => SetPropertyValue(ModeProperty, value); }

    /// <summary>
    /// Gets or sets the destination input (slot 0). May be null (unconnected).
    /// </summary>
    public IGraphicsEffectSource? Destination { get => GetSource(0); set => SetSource(0, value); }

    /// <summary>
    /// Gets or sets the source input (slot 1). May be null (unconnected).
    /// </summary>
    public IGraphicsEffectSource? Source { get => GetSource(1); set => SetSource(1, value); }
}