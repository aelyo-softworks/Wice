using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1TintString)]
    public class TintEffect : EffectWithSource
    {
        public static EffectProperty ColorProperty = EffectProperty.Add(typeof(TintEffect), nameof(Color), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR4, new D2D_VECTOR_4F(1f, 1f, 1f, 1f));
        public static EffectProperty ClampOutputProperty = EffectProperty.Add(typeof(TintEffect), nameof(ClampOutput), 1, false);

        public D2D_VECTOR_4F Color { get => (D2D_VECTOR_4F)GetPropertyValue(ColorProperty); set => SetPropertyValue(ColorProperty, value); }
        public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty); set => SetPropertyValue(ClampOutputProperty, value); }
    }
}
