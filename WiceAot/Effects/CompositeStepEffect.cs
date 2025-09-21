namespace Wice.Effects;

/// <summary>
/// Direct2D composite effect node that blends two input sources using a configurable mode.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1CompositeString)]
#else
[Guid(Constants.CLSID_D2D1CompositeString)]
#endif
public partial class CompositeStepEffect : Effect
{
    /// <summary>
    /// Descriptor for the <see cref="Mode"/> effect parameter.
    /// </summary>
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