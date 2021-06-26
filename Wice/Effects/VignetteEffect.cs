using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1VignetteString)]
    public class VignetteEffect : EffectWithSource
    {
        public static EffectProperty ColorProperty = EffectProperty.Add(typeof(VignetteEffect), nameof(Color), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLOR_TO_VECTOR4, _D3DCOLORVALUE.Black);
        public static EffectProperty TransitionSizeProperty = EffectProperty.Add(typeof(VignetteEffect), nameof(TransitionSize), 1, 0f);
        public static EffectProperty StrengthProperty = EffectProperty.Add(typeof(VignetteEffect), nameof(Strength), 2, 0f);

        public _D3DCOLORVALUE Color { get => (_D3DCOLORVALUE)GetPropertyValue(ColorProperty); set => SetPropertyValue(ColorProperty, value); }
        public float TransitionSize { get => (float)GetPropertyValue(TransitionSizeProperty); set => SetPropertyValue(TransitionSizeProperty, value); }
        public float Strength { get => (float)GetPropertyValue(StrengthProperty); set => SetPropertyValue(StrengthProperty, value); }
    }
}
