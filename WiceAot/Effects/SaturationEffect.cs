namespace Wice.Effects;

/// <summary>
/// Represents the Direct2D Saturation effect.
/// </summary>
/// <remarks>
/// - This effect expects at least one <see cref="IGraphicsEffectSource"/> input (via <see cref="EffectWithSource.Source"/>).<br/>
/// - The single property exposed by this effect is mapped at effect parameter index 0.<br/>
/// - CLSID is provided through the <see cref="System.Runtime.InteropServices.GuidAttribute"/> to enable D2D interop.
/// </remarks>
/// <seealso cref="EffectWithSource"/>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1SaturationString)]
#else
[Guid(Constants.CLSID_D2D1SaturationString)]
#endif
public partial class SaturationEffect : EffectWithSource
{
    /// <summary>
    /// Effect property descriptor for <see cref="Saturation"/>.
    /// </summary>
    /// <remarks>
    /// - D2D property index: 0.<br/>
    /// - Default value: 0.5f.<br/>
    /// - Mapping: <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_DIRECT"/>.
    /// </remarks>
    public static EffectProperty SaturationProperty { get; }

    static SaturationEffect()
    {
        // Register property metadata: index 0 with a 0.5f default value.
        SaturationProperty = EffectProperty.Add(typeof(SaturationEffect), nameof(Saturation), 0, 0.5f);
    }

    /// <summary>
    /// Gets or sets the saturation amount applied by the effect.
    /// </summary>
    /// <value>
    /// A value clamped to the [0, 2] range where:
    /// - 0 produces a completely desaturated (grayscale) image,
    /// - 1 preserves the original saturation,
    /// - 2 applies increased saturation beyond the original.
    /// </value>
    /// <remarks>
    /// Although some documentation references a [0, 1] range, the underlying implementation supports [0, 2].
    /// </remarks>
    public float Saturation { get => (float)GetPropertyValue(SaturationProperty)!; set => SetPropertyValue(SaturationProperty, value.Clamp(0f, 2f)); }
}
