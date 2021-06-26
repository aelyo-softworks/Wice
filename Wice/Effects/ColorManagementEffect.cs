﻿using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1ColorManagementString)]
    public class ColorManagementEffect : EffectWithSource
    {
        public static EffectProperty SourceColorContextProperty = EffectProperty.Add<ID2D1ColorContext>(typeof(ColorManagementEffect), nameof(SourceColorContext), 0);
        public static EffectProperty SourceRenderingIntentProperty = EffectProperty.Add(typeof(ColorManagementEffect), nameof(SourceRenderingIntent), 1, D2D1_COLORMANAGEMENT_RENDERING_INTENT.D2D1_COLORMANAGEMENT_RENDERING_INTENT_PERCEPTUAL);
        public static EffectProperty DestinationColorContextProperty = EffectProperty.Add<ID2D1ColorContext>(typeof(ColorManagementEffect), nameof(DestinationColorContext), 2);
        public static EffectProperty DestinationRenderingIntentProperty = EffectProperty.Add(typeof(ColorManagementEffect), nameof(DestinationRenderingIntent), 3, D2D1_COLORMANAGEMENT_RENDERING_INTENT.D2D1_COLORMANAGEMENT_RENDERING_INTENT_PERCEPTUAL);
        public static EffectProperty AlphaModeProperty = EffectProperty.Add(typeof(ColorManagementEffect), nameof(AlphaMode), 4, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLORMATRIX_ALPHA_MODE, D2D1_COLORMANAGEMENT_ALPHA_MODE.D2D1_COLORMANAGEMENT_ALPHA_MODE_PREMULTIPLIED);
        public static EffectProperty QualityProperty = EffectProperty.Add(typeof(ColorManagementEffect), nameof(Quality), 5, D2D1_COLORMANAGEMENT_QUALITY.D2D1_COLORMANAGEMENT_QUALITY_NORMAL);

        public ID2D1ColorContext SourceColorContext { get => (ID2D1ColorContext)GetPropertyValue(SourceColorContextProperty); set => SetPropertyValue(SourceColorContextProperty, value); }
        public D2D1_COLORMANAGEMENT_RENDERING_INTENT SourceRenderingIntent { get => (D2D1_COLORMANAGEMENT_RENDERING_INTENT)GetPropertyValue(SourceRenderingIntentProperty); set => SetPropertyValue(SourceRenderingIntentProperty, value); }
        public ID2D1ColorContext DestinationColorContext { get => (ID2D1ColorContext)GetPropertyValue(DestinationColorContextProperty); set => SetPropertyValue(DestinationColorContextProperty, value); }
        public D2D1_COLORMANAGEMENT_RENDERING_INTENT DestinationRenderingIntent { get => (D2D1_COLORMANAGEMENT_RENDERING_INTENT)GetPropertyValue(DestinationRenderingIntentProperty); set => SetPropertyValue(DestinationRenderingIntentProperty, value); }
        public D2D1_COLORMANAGEMENT_ALPHA_MODE AlphaMode { get => (D2D1_COLORMANAGEMENT_ALPHA_MODE)GetPropertyValue(AlphaModeProperty); set => SetPropertyValue(AlphaModeProperty, value); }
        public D2D1_COLORMANAGEMENT_QUALITY Quality { get => (D2D1_COLORMANAGEMENT_QUALITY)GetPropertyValue(QualityProperty); set => SetPropertyValue(QualityProperty, value); }
    }
}
