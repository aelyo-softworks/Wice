namespace Wice.Effects;

/// <summary>
/// Applies a uniform opacity factor to the effect's primary <see cref="EffectWithSource.Source"/>.
/// </summary>
/// <remarks>
/// - Wraps the built-in Direct2D "Opacity" effect (CLSID_D2D1Opacity).<br/>
/// - Requires at least one input; enforced by <see cref="EffectWithSource"/>.<br/>
/// - Exposes a single parameter (<see cref="Opacity"/>), mapped at effect parameter index 0.
/// </remarks>
/// <seealso cref="EffectWithSource"/>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1OpacityString)]
#else
[Guid(Constants.CLSID_D2D1OpacityString)]
#endif
public partial class OpacityEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for the <see cref="Opacity"/> property used by the effect pipeline.
    /// </summary>
    /// <remarks>
    /// - Effect parameter index: 0<br/>
    /// - Mapping: <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT"/> (implicit default)<br/>
    /// - Default value: 1.0f (fully opaque)
    /// </remarks>
    public static EffectProperty OpacityProperty { get; }

    // Registers the effect property (index 0, default 1.0f) into the global registry.
    static OpacityEffect()
    {
        OpacityProperty = EffectProperty.Add(typeof(OpacityEffect), nameof(Opacity), 0, 1f);
    }

    /// <summary>
    /// Gets or sets the uniform opacity factor applied to the source.
    /// </summary>
    /// <value>
    /// A value typically in the [0, 1] range, where 0 is fully transparent and 1 is fully opaque.
    /// </value>
    /// <remarks>
    /// The value is forwarded as-is to the underlying effect; callers should clamp as needed.
    /// </remarks>
    public float Opacity { get => (float)GetPropertyValue(OpacityProperty)!; set => SetPropertyValue(OpacityProperty, value); }
}
