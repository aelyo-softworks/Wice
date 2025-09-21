namespace Wice.Effects;

/// <summary>
/// Represents the Direct2D "Composite" effect, which combines multiple input sources
/// using a specified <see cref="D2D1_COMPOSITE_MODE"/> (e.g., SourceOver, Multiply, etc.).
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1CompositeString)]
#else
[Guid(Constants.CLSID_D2D1CompositeString)]
#endif
public partial class CompositeEffect : Effect
{
    /// <summary>
    /// Describes the effect property that controls the compositing mode.
    /// </summary>
    public static EffectProperty ModeProperty { get; }

    static CompositeEffect()
    {
        // Property 0 -> Mode (DIRECT mapping, default SourceOver)
        ModeProperty = EffectProperty.Add(typeof(CompositeEffect), nameof(Mode), 0, D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CompositeEffect"/> that may accept an unbounded number of sources.
    /// </summary>
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
