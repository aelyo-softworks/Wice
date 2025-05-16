namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1OpacityMetadataString)]
#else
[Guid(Constants.CLSID_D2D1OpacityMetadataString)]
#endif
public partial class OpacityMetadataEffect : EffectWithSource
{
    public static EffectProperty InputOpaqueRectProperty { get; }

    static OpacityMetadataEffect()
    {
        InputOpaqueRectProperty = EffectProperty.Add(typeof(OpacityMetadataEffect), nameof(InputOpaqueRect), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4, new D2D_VECTOR_4F(float.MinValue, float.MinValue, float.MaxValue, float.MinValue));
    }

    public D2D_VECTOR_4F InputOpaqueRect { get => (D2D_VECTOR_4F)GetPropertyValue(InputOpaqueRectProperty)!; set => SetPropertyValue(InputOpaqueRectProperty, value); }
}
