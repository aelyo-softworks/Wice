namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1TintString)]
#else
[Guid(Constants.CLSID_D2D1TintString)]
#endif
/// <summary>
/// Direct2D/Win2D Tint effect wrapper.
/// Applies a color tint to the input source and optionally clamps the output to [0,1].
/// </summary>
/// <remarks>
/// Exposes two parameters:
/// - <see cref="Color"/> (index 0), mapped as <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR4"/>.
/// - <see cref="ClampOutput"/> (index 1), mapped as DIRECT boolean.
/// The CLSID is provided via <see cref="GuidAttribute"/> and varies between .NET Framework and .NET targets.
/// </remarks>
/// <seealso cref="EffectWithSource"/>
/// <seealso cref="EffectProperty"/>
public partial class TintEffect : EffectWithSource
{
    /// <summary>
    /// Descriptor for the <see cref="Color"/> effect parameter.
    /// Index: 0. Mapping: COLOR_TO_VECTOR4. Default: (1,1,1,1).
    /// </summary>
    public static EffectProperty ColorProperty { get; }

    /// <summary>
    /// Descriptor for the <see cref="ClampOutput"/> effect parameter.
    /// Index: 1. Mapping: DIRECT. Default: <see langword="false"/>.
    /// </summary>
    public static EffectProperty ClampOutputProperty { get; }

    static TintEffect()
    {
        ColorProperty = EffectProperty.Add(typeof(TintEffect), nameof(Color), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR4, new D2D_VECTOR_4F(1f, 1f, 1f, 1f));
        ClampOutputProperty = EffectProperty.Add(typeof(TintEffect), nameof(ClampOutput), 1, false);
    }

    /// <summary>
    /// Gets or sets the tint color as a premultiplied RGBA vector in the [0,1] range.
    /// </summary>
    /// <remarks>
    /// Serialized at index 0 and mapped to a D2D vector4 via COLOR_TO_VECTOR4.
    /// </remarks>
    public D2D_VECTOR_4F Color { get => (D2D_VECTOR_4F)GetPropertyValue(ColorProperty)!; set => SetPropertyValue(ColorProperty, value); }

    /// <summary>
    /// Gets or sets whether the effect clamps output channels to the [0,1] range.
    /// </summary>
    /// <remarks>
    /// Serialized at index 1 using a DIRECT boolean mapping.
    /// </remarks>
    public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty)!; set => SetPropertyValue(ClampOutputProperty, value); }
}
