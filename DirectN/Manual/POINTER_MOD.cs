using System;

namespace DirectN
{
    [Flags]
    public enum POINTER_MOD
    {
        POINTER_MOD_NONE = 0x0000,
        POINTER_MOD_LBUTTON = 0x0001,
        POINTER_MOD_RBUTTON = 0x0002,
        POINTER_MOD_SHIFT = 0x0004,    // Shift key is held down.
        POINTER_MOD_CTRL = 0x0008,    // Ctrl key is held down.
        POINTER_MOD_MBUTTON = 0x0010,
        POINTER_MOD_XBUTTON1 = 0x0020,
        POINTER_MOD_XBUTTON2 = 0x0040,
    }
}
