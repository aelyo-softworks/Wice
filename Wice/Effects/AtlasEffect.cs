using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1AtlasString)]
    public class AtlasEffect : EffectWithSource
    {
        public static EffectProperty InputRectProperty { get; }
        public static EffectProperty InputPaddingRectProperty { get; }

        static AtlasEffect()
        {
            InputRectProperty = EffectProperty.Add(typeof(AtlasEffect), nameof(InputRect), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4, new D2D_VECTOR_4F(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue));
            InputPaddingRectProperty = EffectProperty.Add(typeof(AtlasEffect), nameof(InputPaddingRect), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4, new D2D_VECTOR_4F(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue));
        }

        public D2D_VECTOR_4F InputRect { get => (D2D_VECTOR_4F)GetPropertyValue(InputRectProperty); set => SetPropertyValue(InputRectProperty, value); }
        public D2D_VECTOR_4F InputPaddingRect { get => (D2D_VECTOR_4F)GetPropertyValue(InputPaddingRectProperty); set => SetPropertyValue(InputPaddingRectProperty, value); }
    }
}
