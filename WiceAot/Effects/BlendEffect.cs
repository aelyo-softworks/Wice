namespace Wice.Effects;

/// <summary>
/// Managed wrapper for the Direct2D Blend effect.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1BlendString)]
#else
[Guid(Constants.CLSID_D2D1BlendString)]
#endif
public partial class BlendEffect : Effect
{
    /// <summary>
    /// Descriptor for the <see cref="Mode"/> effect parameter.
    /// </summary>
    public static EffectProperty ModeProperty { get; }

    static BlendEffect()
    {
        // Register the Mode parameter as effect property index 0 with a default of MULTIPLY.
        ModeProperty = EffectProperty.Add(typeof(BlendEffect), nameof(Mode), 0, D2D1_BLEND_MODE.D2D1_BLEND_MODE_MULTIPLY);
    }

    /// <summary>
    /// Initializes a new instance configured with two sources: <see cref="Background"/> and <see cref="Foreground"/>.
    /// </summary>
    public BlendEffect()
            : base(2)
    {
    }

    /// <summary>
    /// Gets or sets the blending mode applied between <see cref="Background"/> and <see cref="Foreground"/>.
    /// </summary>
    public D2D1_BLEND_MODE Mode { get => (D2D1_BLEND_MODE)GetPropertyValue(ModeProperty)!; set => SetPropertyValue(ModeProperty, value); }

    /// <summary>
    /// Gets or sets the background source (effect source index 0).
    /// </summary>
    public IGraphicsEffectSource? Background { get => GetSource(0); set => SetSource(0, value); }

    /// <summary>
    /// Gets or sets the foreground source (effect source index 1).
    /// </summary>
    public IGraphicsEffectSource? Foreground { get => GetSource(1); set => SetSource(1, value); }
}
