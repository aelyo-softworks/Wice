﻿namespace Wice.Effects;

#if NETFRAMEWORK
[Guid(D2D1Constants.CLSID_D2D1InvertString)]
#else
[Guid(Constants.CLSID_D2D1InvertString)]
#endif
public partial class InvertEffect : EffectWithSource
{
}
