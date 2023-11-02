using System;

namespace Wice
{
    public class PointerDragEventArgs : PointerUpdateEventArgs
    {
        public PointerDragEventArgs(PointerUpdateEventArgs e, DragState state)
            : base((e?.PointerId).GetValueOrDefault(), (e?.X).GetValueOrDefault(), (e?.Y).GetValueOrDefault(), (e?.Flags).GetValueOrDefault())
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            State = state;
        }

        public DragState State { get; }
    }
}
