using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1ArithmeticCompositeString)]
    public class ArithmeticCompositeEffect : EffectWithTwoSources
    {
        public static EffectProperty CoefficientsProperty = EffectProperty.Add(typeof(ArithmeticCompositeEffect), nameof(Coefficients), 0, new D2D_VECTOR_4F(1f, 0f, 0f, 0f));
        public static EffectProperty ClampOutputProperty = EffectProperty.Add(typeof(ArithmeticCompositeEffect), nameof(ClampOutput), 1, false);

        public D2D_VECTOR_4F Coefficients { get => (D2D_VECTOR_4F)GetPropertyValue(CoefficientsProperty); set => SetPropertyValue(CoefficientsProperty, value); }
        public bool ClampOutput { get => (bool)GetPropertyValue(ClampOutputProperty); set => SetPropertyValue(ClampOutputProperty, value); }
    }
}
