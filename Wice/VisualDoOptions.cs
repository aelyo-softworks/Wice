using System;

namespace Wice
{
    [Flags]
    public enum VisualDoOptions
    {
        None = 0x0,
        ImmediateOnly = 0x1,
        DeferredOnly = 0x2,
    }
}
