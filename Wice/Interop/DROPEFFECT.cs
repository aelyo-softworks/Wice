using System;

namespace Wice.Interop
{
    [Flags]
    public enum DROPEFFECT
    {
        DROPEFFECT_NONE = 0,
        DROPEFFECT_COPY = 1,
        DROPEFFECT_MOVE = 2,
        DROPEFFECT_LINK = 4,
        DROPEFFECT_SCROLL = unchecked((int)0x80000000),
    }
}
