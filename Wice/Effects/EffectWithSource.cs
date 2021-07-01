using System;
#if NET
using IGraphicsEffectSource = DirectN.IGraphicsEffectSourceWinRT;
#else
using IGraphicsEffectSource = Windows.Graphics.Effects.IGraphicsEffectSource;
#endif

namespace Wice.Effects
{
    public abstract class EffectWithSource : Effect
    {
        protected EffectWithSource(int sourcesCount = 1)
            : base(sourcesCount)
        {
            if (sourcesCount < 1)
                throw new ArgumentOutOfRangeException(nameof(sourcesCount));
        }

        public IGraphicsEffectSource Source { get => GetSource(0); set => SetSource(0, value); }
    }
}
