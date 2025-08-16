namespace Wice.Effects;

/// <summary>
/// Managed wrapper for the Direct2D Blend effect.
/// </summary>
/// <remarks>
/// - Exposes two sources:
///   - <see cref="Background"/> (index 0)
///   - <see cref="Foreground"/> (index 1)
/// - Controls the blend operation through <see cref="Mode"/>.
/// - Properties are surfaced to D2D via <see cref="EffectProperty"/> descriptors on the type.
/// </remarks>
/// <seealso cref="Effect"/>
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
    /// <remarks>
    /// - Effect property index: 0 (serialized into the D2D property bag at position 0).<br/>
    /// - Mapping: Direct (<see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT"/>).<br/>
    /// - Default: <see cref="D2D1_BLEND_MODE.D2D1_BLEND_MODE_MULTIPLY"/>.
    /// </remarks>
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
    /// <remarks>
    /// Default is <see cref="D2D1_BLEND_MODE.D2D1_BLEND_MODE_MULTIPLY"/>. See <see cref="D2D1_BLEND_MODE"/> for supported modes.
    /// </remarks>
    public D2D1_BLEND_MODE Mode { get => (D2D1_BLEND_MODE)GetPropertyValue(ModeProperty)!; set => SetPropertyValue(ModeProperty, value); }

    /// <summary>
    /// Gets or sets the background source (effect source index 0).
    /// </summary>
    /// <remarks>
    /// May be <see langword="null"/>. Uses <see cref="Effect.GetSource(int)"/> and <see cref="Effect.SetSource(int, IGraphicsEffectSource?)"/>.
    /// </remarks>
    public IGraphicsEffectSource? Background { get => GetSource(0); set => SetSource(0, value); }

    /// <summary>
    /// Gets or sets the foreground source (effect source index 1).
    /// </summary>
    /// <remarks>
    /// May be <see langword="null"/>. Uses <see cref="Effect.GetSource(int)"/> and <see cref="Effect.SetSource(int, IGraphicsEffectSource?)"/>.
    /// </remarks>
    public IGraphicsEffectSource? Foreground { get => GetSource(1); set => SetSource(1, value); }
}
