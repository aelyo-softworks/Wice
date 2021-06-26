using System;
using System.Runtime.InteropServices;
using DirectN;
using Windows.Graphics.Effects;

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1CompositeString)]
    public class CompositeStepEffect : Effect
    {
        public static EffectProperty ModeProperty = EffectProperty.Add(typeof(CompositeStepEffect), nameof(Mode), 0, D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER);

        public CompositeStepEffect()
            : base(2)
        {
        }

        public D2D1_COMPOSITE_MODE Mode { get => (D2D1_COMPOSITE_MODE)GetPropertyValue(ModeProperty); set => SetPropertyValue(ModeProperty, value); }

        public IGraphicsEffectSource Destination { get => GetSource(0); set => SetSource(0, value); }
        public IGraphicsEffectSource Source { get => GetSource(1); set => SetSource(1, value); }
    }
}
