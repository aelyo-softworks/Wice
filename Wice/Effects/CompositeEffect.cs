using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1CompositeString)]
    public class CompositeEffect : Effect
    {
        public static EffectProperty ModeProperty = EffectProperty.Add(typeof(CompositeEffect), nameof(Mode), 0, D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER);

        public CompositeEffect()
            : base(int.MaxValue)
        {
        }

        public D2D1_COMPOSITE_MODE Mode { get => (D2D1_COMPOSITE_MODE)GetPropertyValue(ModeProperty); set => SetPropertyValue(ModeProperty, value); }
    }
}
