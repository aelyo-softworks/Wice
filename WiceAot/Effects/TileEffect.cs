﻿namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1TileString)]
#else
[Guid(Constants.CLSID_D2D1TileString)]
#endif
public partial class TileEffect : EffectWithSource
{
    public static EffectProperty RectProperty { get; }

    static TileEffect()
    {
        RectProperty = EffectProperty.Add(typeof(TileEffect), nameof(Rect), 0, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_RECT_TO_VECTOR4, new D2D_VECTOR_4F(0f, 0f, 100f, 100f));
    }

    public D2D_VECTOR_4F Rect { get => (D2D_VECTOR_4F)GetPropertyValue(RectProperty)!; set => SetPropertyValue(RectProperty, value); }
}
