namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1ColorMatrixString)]
#else
[Guid(Constants.CLSID_D2D1ColorMatrixString)]
#endif
public partial class ColorMatrixEffect : EffectWithSource
{
    public static EffectProperty ColorMatrixProperty { get; }
    public static EffectProperty AlphaModeProperty { get; }
    public static EffectProperty ClampOutputProperty { get; }

    static ColorMatrixEffect()
    {
        ColorMatrixProperty = EffectProperty.Add(typeof(ColorMatrixEffect), nameof(ColorMatrix), 0, new D2D_MATRIX_5X4_F(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f));
        AlphaModeProperty = EffectProperty.Add(typeof(ColorMatrixEffect), nameof(AlphaMode), 1, GRAPHICS_EFFECT_PROPERTY_MAPPING.GRAPHICS_EFFECT_PROPERTY_MAPPING_COLORMATRIX_ALPHA_MODE, D2D1_COLORMATRIX_ALPHA_MODE.D2D1_COLORMATRIX_ALPHA_MODE_PREMULTIPLIED);
        ClampOutputProperty = EffectProperty.Add(typeof(ColorMatrixEffect), nameof(ClampOutput), 2, false);
    }

    public D2D_MATRIX_5X4_F ColorMatrix { get => (D2D_MATRIX_5X4_F)GetPropertyValue(ColorMatrixProperty)!; set => SetPropertyValue(ColorMatrixProperty, value); }
    public D2D1_COLORMATRIX_ALPHA_MODE AlphaMode { get => (D2D1_COLORMATRIX_ALPHA_MODE)GetPropertyValue(AlphaModeProperty)!; set => SetPropertyValue(AlphaModeProperty, value); }
    public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty)!; set => SetPropertyValue(ClampOutputProperty, value); }
}
