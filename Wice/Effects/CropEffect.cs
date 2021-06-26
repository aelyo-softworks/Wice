﻿using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1CropString)]
    public class CropEffect : EffectWithSource
    {
        public static EffectProperty RectProperty = EffectProperty.Add(typeof(CropEffect), nameof(Rect), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4, new D2D_VECTOR_4F(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue));
        public static EffectProperty BorderModeProperty = EffectProperty.Add(typeof(CropEffect), nameof(BorderMode), 1, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);

        public D2D_VECTOR_4F Rect { get => (D2D_VECTOR_4F)GetPropertyValue(RectProperty); set => SetPropertyValue(RectProperty, value); }
        public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty); set => SetPropertyValue(BorderModeProperty, value); }
    }
}
