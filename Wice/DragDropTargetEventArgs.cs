using System;

namespace Wice
{
    public class DragDropTargetEventArgs : EventArgs
    {
        public DragDropTargetEventArgs(DragDropTargetEventType type, IntPtr hwnd)
        {
            Type = type;
            Hwnd = hwnd;
        }

        public DragDropTargetEventType Type { get; }
        public IntPtr Hwnd { get; }
    }
}
