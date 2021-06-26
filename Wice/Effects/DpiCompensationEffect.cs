﻿using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1DpiCompensationString)]
    public class DpiCompensationEffect : EffectWithSource
    {
        public static EffectProperty InterpolationModeProperty = EffectProperty.Add(typeof(DpiCompensationEffect), nameof(InterpolationMode), 0, D2D1_DPICOMPENSATION_INTERPOLATION_MODE.D2D1_DPICOMPENSATION_INTERPOLATION_MODE_LINEAR);
        public static EffectProperty BorderModeProperty = EffectProperty.Add(typeof(DpiCompensationEffect), nameof(BorderMode), 1, D2D1_BORDER_MODE.D2D1_BORDER_MODE_SOFT);
        public static EffectProperty InputDpiProperty = EffectProperty.Add(typeof(DpiCompensationEffect), nameof(InputDpi), 2, 96f);

        public D2D1_DPICOMPENSATION_INTERPOLATION_MODE InterpolationMode { get => (D2D1_DPICOMPENSATION_INTERPOLATION_MODE)GetPropertyValue(InterpolationModeProperty); set => SetPropertyValue(InterpolationModeProperty, value); }
        public D2D1_BORDER_MODE BorderMode { get => (D2D1_BORDER_MODE)GetPropertyValue(BorderModeProperty); set => SetPropertyValue(BorderModeProperty, value); }
        public float InputDpi { get => (float)GetPropertyValue(InputDpiProperty); set => SetPropertyValue(InputDpiProperty, value.Clamp(0f, 360f)); }
    }
}
