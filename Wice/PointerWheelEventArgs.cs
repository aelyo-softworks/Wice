﻿using DirectN;

namespace Wice
{
    public class PointerWheelEventArgs : PointerPositionEventArgs
    {
        public PointerWheelEventArgs(int pointerId, int x, int y, int delta, Orientation orientation)
            : base(pointerId, x, y)
        {
            Delta = delta / WindowsConstants.WHEEL_DELTA;
            Orientation = orientation;
        }

        public int Delta { get; }
        public Orientation Orientation { get; }

        public override string ToString() => base.ToString() + ",DE=" + Delta + ",O=" + Orientation;
    }
}