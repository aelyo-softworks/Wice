namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1LookupTable3DString)]
#else
[Guid(Constants.CLSID_D2D1LookupTable3DString)]
#endif
public partial class LookupTable3DEffect : EffectWithSource
{
    public static EffectProperty LutProperty { get; }
    public static EffectProperty AlphaModeProperty { get; }

    static LookupTable3DEffect()
    {
        LutProperty = EffectProperty.Add<object>(typeof(LookupTable3DEffect), nameof(Lut), 0);
        AlphaModeProperty = EffectProperty.Add(typeof(LookupTable3DEffect), nameof(AlphaMode), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLORMATRIX_ALPHA_MODE, D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED);
    }

    public object Lut { get => GetPropertyValue(LutProperty)!; set => SetPropertyValue(LutProperty, value); }
    public D2D1_ALPHA_MODE AlphaMode { get => (D2D1_ALPHA_MODE)GetPropertyValue(AlphaModeProperty)!; set => SetPropertyValue(AlphaModeProperty, value); }
}
