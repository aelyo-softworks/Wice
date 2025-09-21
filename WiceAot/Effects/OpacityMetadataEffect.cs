namespace Wice.Effects;

/// <summary>
/// Direct2D OpacityMetadata effect wrapper.
/// Declares the portion of the input that is guaranteed to be fully opaque, allowing the pipeline
/// to skip unnecessary blending work when possible.
/// </summary>
#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1OpacityMetadataString)]
#else
[Guid(Constants.CLSID_D2D1OpacityMetadataString)]
#endif
public partial class OpacityMetadataEffect : EffectWithSource
{
    /// <summary>
    /// Effect property descriptor for <see cref="InputOpaqueRect"/>.
    /// Index: 0, Mapping: <see cref="GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4"/>.
    /// Default: <c>(float.MinValue, float.MinValue, float.MaxValue, float.MinValue)</c> (sentinel extremes).
    /// </summary>
    public static EffectProperty InputOpaqueRectProperty { get; }

    /// <summary>
    /// Static initializer that registers effect properties with the global registry.
    /// </summary>
    static OpacityMetadataEffect()
    {
        InputOpaqueRectProperty = EffectProperty.Add(
            typeof(OpacityMetadataEffect),
            nameof(InputOpaqueRect),
            index: 0,
            mapping: GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4,
            defaultValue: new D2D_VECTOR_4F(float.MinValue, float.MinValue, float.MaxValue, float.MinValue)
        );
    }

    /// <summary>
    /// Gets or sets the rectangle (in DIPs) that is fully opaque in the input.
    /// Stored as a <see cref="D2D_VECTOR_4F"/> representing (Left, Top, Right, Bottom).
    /// </summary>
    public D2D_VECTOR_4F InputOpaqueRect
    {
        get => (D2D_VECTOR_4F)GetPropertyValue(InputOpaqueRectProperty)!;
        set => SetPropertyValue(InputOpaqueRectProperty, value);
    }
}
