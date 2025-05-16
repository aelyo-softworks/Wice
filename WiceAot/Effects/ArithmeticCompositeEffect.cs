namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1ArithmeticCompositeString)]
#else
[Guid(Constants.CLSID_D2D1ArithmeticCompositeString)]
#endif
public partial class ArithmeticCompositeEffect : EffectWithTwoSources
{
    public static EffectProperty CoefficientsProperty { get; }
    public static EffectProperty ClampOutputProperty { get; }

    static ArithmeticCompositeEffect()
    {
        CoefficientsProperty = EffectProperty.Add(typeof(ArithmeticCompositeEffect), nameof(Coefficients), 0, new D2D_VECTOR_4F(1f, 0f, 0f, 0f));
        ClampOutputProperty = EffectProperty.Add(typeof(ArithmeticCompositeEffect), nameof(ClampOutput), 1, false);
    }

    public D2D_VECTOR_4F Coefficients { get => (D2D_VECTOR_4F)GetPropertyValue(CoefficientsProperty)!; set => SetPropertyValue(CoefficientsProperty, value); }
    public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty)!; set => SetPropertyValue(ClampOutputProperty, value); }
}
