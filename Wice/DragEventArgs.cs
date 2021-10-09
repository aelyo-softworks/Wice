using System;
using DirectN;

namespace Wice
{
    public class DragEventArgs : EventArgs
    {
        public DragEventArgs(int x, int y, POINTER_MOD keys, DragState state, EventArgs sourceEventArgs = null)
        {
            X = x;
            Y = y;
            Keys = keys;
            State = state;
            SourceEventArgs = sourceEventArgs;
        }

        public POINTER_MOD Keys { get; }
        public EventArgs SourceEventArgs { get; }

        // window relative
        public int X { get; }
        public int Y { get; }

        public DragState State { get; }
    }
}
