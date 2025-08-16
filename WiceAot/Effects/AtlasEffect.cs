namespace Wice.Effects;

/// <summary>
/// Wraps the D2D Atlas effect, which crops the primary input to a rectangle and defines
/// an additional padding rectangle used to avoid sampling artifacts at the edges.
/// </summary>
/// <remarks>
/// - Requires a source; see <see cref="EffectWithSource.Source"/>.
/// - Properties are exposed as <see cref="D2D_VECTOR_4F"/> mapped with
///   <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4"/>:
///   (Left, Top, Right, Bottom).
/// - Default values are effectively "unbounded" (MinValue..MaxValue), so the source is unchanged until set.
/// </remarks>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1AtlasString)]
#else
[Guid(Constants.CLSID_D2D1AtlasString)]
#endif
public partial class AtlasEffect : EffectWithSource
{
    /// <summary>
    /// Effect property descriptor for <see cref="InputRect"/> (effect parameter index 0).
    /// </summary>
    public static EffectProperty InputRectProperty { get; }

    /// <summary>
    /// Effect property descriptor for <see cref="InputPaddingRect"/> (effect parameter index 1).
    /// </summary>
    public static EffectProperty InputPaddingRectProperty { get; }

    /// <summary>
    /// Registers the effect properties and their defaults.
    /// </summary>
    /// <remarks>
    /// Both properties are mapped as RECT to VECTOR4 and default to an "infinite" rectangle
    /// (MinValue, MinValue, MaxValue, MaxValue).
    /// </remarks>
    static AtlasEffect()
    {
        InputRectProperty = EffectProperty.Add(typeof(AtlasEffect), nameof(InputRect), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4, new D2D_VECTOR_4F(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue));
        InputPaddingRectProperty = EffectProperty.Add(typeof(AtlasEffect), nameof(InputPaddingRect), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4, new D2D_VECTOR_4F(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue));
    }

    /// <summary>
    /// Gets or sets the source rectangle, in DIPs, used to crop the primary input.
    /// </summary>
    /// <remarks>
    /// Stored as (Left, Top, Right, Bottom). When left at its default (MinValue..MaxValue), no cropping is applied.
    /// </remarks>
    public D2D_VECTOR_4F InputRect { get => (D2D_VECTOR_4F)GetPropertyValue(InputRectProperty)!; set => SetPropertyValue(InputRectProperty, value); }

    /// <summary>
    /// Gets or sets the padding rectangle, in DIPs, used for edge sampling outside <see cref="InputRect"/>.
    /// </summary>
    /// <remarks>
    /// Stored as (Left, Top, Right, Bottom). This helps avoid texture bleeding by defining a region from which pixels
    /// are sampled when filtering near the edges of <see cref="InputRect"/>.
    /// </remarks>
    public D2D_VECTOR_4F InputPaddingRect { get => (D2D_VECTOR_4F)GetPropertyValue(InputPaddingRectProperty)!; set => SetPropertyValue(InputPaddingRectProperty, value); }
}
