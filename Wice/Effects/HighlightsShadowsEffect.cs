﻿using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1HighlightsShadowsString)]
    public class HighlightsShadowsEffect : EffectWithSource
    {
        public static EffectProperty HighlightsProperty = EffectProperty.Add(typeof(HighlightsShadowsEffect), nameof(Highlights), 0, 0f);
        public static EffectProperty ShadowsProperty = EffectProperty.Add(typeof(HighlightsShadowsEffect), nameof(Shadows), 1, 0f);
        public static EffectProperty ClarityProperty = EffectProperty.Add(typeof(HighlightsShadowsEffect), nameof(Clarity), 2, 0f);
        public static EffectProperty InputGammaProperty = EffectProperty.Add(typeof(HighlightsShadowsEffect), nameof(InputGamma), 3, D2D1_HIGHLIGHTSANDSHADOWS_INPUT_GAMMA.D2D1_HIGHLIGHTSANDSHADOWS_INPUT_GAMMA_LINEAR);
        public static EffectProperty MaskBlurRadiusProperty = EffectProperty.Add(typeof(HighlightsShadowsEffect), nameof(MaskBlurRadius), 4, 1.25f);

        public float Highlights { get => (float)GetPropertyValue(HighlightsProperty); set => SetPropertyValue(HighlightsProperty, value.Clamp(-1f, 1f)); }
        public float Shadows { get => (float)GetPropertyValue(ShadowsProperty); set => SetPropertyValue(ShadowsProperty, value.Clamp(-1f, 1f)); }
        public float Clarity { get => (float)GetPropertyValue(ClarityProperty); set => SetPropertyValue(ClarityProperty, value.Clamp(-1f, 1f)); }
        public D2D1_HIGHLIGHTSANDSHADOWS_INPUT_GAMMA InputGamma { get => (D2D1_HIGHLIGHTSANDSHADOWS_INPUT_GAMMA)GetPropertyValue(InputGammaProperty); set => SetPropertyValue(InputGammaProperty, value); }
        public float MaskBlurRadius { get => (float)GetPropertyValue(MaskBlurRadiusProperty); set => SetPropertyValue(MaskBlurRadiusProperty, value.Clamp(0f, 10f)); }
    }
}
