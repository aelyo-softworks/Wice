﻿namespace Wice;

public class PointerLeaveEventArgs : PointerUpdateEventArgs
{
    public PointerLeaveEventArgs(uint pointerId, int x, int y, POINTER_MESSAGE_FLAGS flags)
        : base(pointerId, x, y, flags)
    {
    }
}
