using System;

namespace Wice
{
    public class PointerDragEventArgs : PointerPositionEventArgs
    {
        public PointerDragEventArgs(PointerPositionEventArgs e, DragState state)
            : base((e?.PointerId).GetValueOrDefault(), (e?.X).GetValueOrDefault(), (e?.Y).GetValueOrDefault())
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            State = state;
        }

        public DragState State { get; }
    }
}
