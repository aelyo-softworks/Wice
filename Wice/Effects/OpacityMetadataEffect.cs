using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1OpacityMetadataString)]
    public class OpacityMetadataEffect : EffectWithSource
    {
        public static EffectProperty InputOpaqueRectProperty = EffectProperty.Add(typeof(OpacityMetadataEffect), nameof(InputOpaqueRect), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4, new D2D_VECTOR_4F(float.MinValue, float.MinValue, float.MaxValue, float.MinValue));

        public D2D_VECTOR_4F InputOpaqueRect { get => (D2D_VECTOR_4F)GetPropertyValue(InputOpaqueRectProperty); set => SetPropertyValue(InputOpaqueRectProperty, value); }
    }
}
