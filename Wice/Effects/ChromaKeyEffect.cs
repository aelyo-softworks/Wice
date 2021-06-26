using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1ChromaKeyString)]
    public class ChromaKeyEffect : EffectWithSource
    {
        public static EffectProperty ColorProperty = EffectProperty.Add(typeof(ChromaKeyEffect), nameof(Color), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR3, new D2D_VECTOR_3F());
        public static EffectProperty ToleranceProperty = EffectProperty.Add(typeof(ChromaKeyEffect), nameof(Tolerance), 1, 0.1f);
        public static EffectProperty InvertAlphaProperty = EffectProperty.Add(typeof(ChromaKeyEffect), nameof(InvertAlpha), 2, false);
        public static EffectProperty FeatherProperty = EffectProperty.Add(typeof(ChromaKeyEffect), nameof(Feather), 3, false);

        public D2D_VECTOR_3F Color { get => (D2D_VECTOR_3F)GetPropertyValue(ColorProperty); set => SetPropertyValue(ColorProperty, value); }
        public float Tolerance { get => (float)GetPropertyValue(ToleranceProperty); set => SetPropertyValue(ToleranceProperty, value.Clamp(0f, 1f)); }
        public bool InvertAlpha { get => (bool)GetPropertyValue(InvertAlphaProperty); set => SetPropertyValue(InvertAlphaProperty, value); }
        public bool Feather { get => (bool)GetPropertyValue(FeatherProperty); set => SetPropertyValue(FeatherProperty, value); }
    }
}
