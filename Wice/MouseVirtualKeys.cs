using System;

namespace Wice
{
    [Flags]
    public enum MouseVirtualKeys
    {
        None = 0,
        LeftButton = 1,
        RightButton = 2,
        Shift = 4,
        Control = 8,
        MiddleButton = 16,
        XButton1 = 32,
        XButton2 = 64,
    }
}
