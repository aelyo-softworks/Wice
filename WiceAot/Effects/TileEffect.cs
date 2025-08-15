namespace Wice.Effects;

/// <summary>
/// Direct2D Tile effect wrapper.
/// </summary>
/// <remarks>
/// - Exposes a single parameter, <see cref="Rect"/>, at effect property index 0.
/// - <see cref="Rect"/> is mapped using <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4"/>
///   for D2D interop (Vector4: Left, Top, Right, Bottom or Left, Top, Width, Height as required by the underlying effect).
/// - Requires at least one input source via <see cref="EffectWithSource.Source"/>.
/// </remarks>
/// <seealso cref="EffectWithSource"/>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1TileString)]
#else
[Guid(Constants.CLSID_D2D1TileString)]
#endif
public partial class TileEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for the <see cref="Rect"/> effect parameter.
    /// </summary>
    /// <remarks>
    /// - Index: 0
    /// - Mapping: <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4"/>
    /// - Default: (0, 0, 100, 100) in DIPs
    /// </remarks>
    public static EffectProperty RectProperty { get; }

    // Registers the Rect effect property (index 0) with D2D mapping and default value.
    static TileEffect()
    {
        RectProperty = EffectProperty.Add(typeof(TileEffect), nameof(Rect), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4, new D2D_VECTOR_4F(0f, 0f, 100f, 100f));
    }

    /// <summary>
    /// Gets or sets the tile rectangle for the effect, in device-independent pixels (DIPs).
    /// </summary>
    /// <remarks>
    /// Serialized to D2D using RECT_TO_VECTOR4 mapping.
    /// </remarks>
    public D2D_VECTOR_4F Rect { get => (D2D_VECTOR_4F)GetPropertyValue(RectProperty)!; set => SetPropertyValue(RectProperty, value); }
}
