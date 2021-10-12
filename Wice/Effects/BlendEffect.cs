using System;
using System.Runtime.InteropServices;
using DirectN;
#if NET
using IGraphicsEffectSource = DirectN.IGraphicsEffectSourceWinRT;
#else
using IGraphicsEffectSource = Windows.Graphics.Effects.IGraphicsEffectSource;
#endif

namespace Wice.Effects
{
    [Guid(D2D1Constants.CLSID_D2D1BlendString)]
    public class BlendEffect : Effect
    {
        public static EffectProperty ModeProperty { get; }

        static BlendEffect()
        {
            ModeProperty = EffectProperty.Add(typeof(BlendEffect), nameof(Mode), 0, D2D1_BLEND_MODE.D2D1_BLEND_MODE_MULTIPLY);
        }

        public BlendEffect()
                : base(2)
        {
        }

        public D2D1_BLEND_MODE Mode { get => (D2D1_BLEND_MODE)GetPropertyValue(ModeProperty); set => SetPropertyValue(ModeProperty, value); }

        public IGraphicsEffectSource Background { get => GetSource(0); set => SetSource(0, value); }
        public IGraphicsEffectSource Foreground { get => GetSource(1); set => SetSource(1, value); }
    }
}
