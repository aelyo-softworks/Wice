using System;

namespace Wice
{
    public class DragDropSourceEventArgs : EventArgs
    {
        public DragDropSourceEventArgs(DragDropSourceEventType type)
        {
            Type = type;
        }

        public DragDropSourceEventType Type { get; }
    }
}
