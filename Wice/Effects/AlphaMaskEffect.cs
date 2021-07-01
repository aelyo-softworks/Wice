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
    [Guid(D2D1Constants.CLSID_D2D1AlphaMaskString)]
    public class AlphaMaskEffect : EffectWithSource
    {
        public AlphaMaskEffect()
            : base(2)
        {
        }

        public IGraphicsEffectSource AlphaMask { get => GetSource(1); set => SetSource(1, value); }
    }
}
