namespace Wice.Effects;

/// <summary>
/// Wraps the D2D Atlas effect, which crops the primary input to a rectangle and defines
/// an additional padding rectangle used to avoid sampling artifacts at the edges.
/// </summary>
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
    static AtlasEffect()
    {
        InputRectProperty = EffectProperty.Add(typeof(AtlasEffect), nameof(InputRect), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4, new D2D_VECTOR_4F(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue));
        InputPaddingRectProperty = EffectProperty.Add(typeof(AtlasEffect), nameof(InputPaddingRect), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4, new D2D_VECTOR_4F(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue));
    }

    /// <summary>
    /// Gets or sets the source rectangle, in DIPs, used to crop the primary input.
    /// </summary>
    public D2D_VECTOR_4F InputRect { get => (D2D_VECTOR_4F)GetPropertyValue(InputRectProperty)!; set => SetPropertyValue(InputRectProperty, value); }

    /// <summary>
    /// Gets or sets the padding rectangle, in DIPs, used for edge sampling outside <see cref="InputRect"/>.
    /// </summary>
    public D2D_VECTOR_4F InputPaddingRect { get => (D2D_VECTOR_4F)GetPropertyValue(InputPaddingRectProperty)!; set => SetPropertyValue(InputPaddingRectProperty, value); }
}
