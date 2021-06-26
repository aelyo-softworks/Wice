namespace Wice
{
    public class MouseDragEventArgs : MouseEventArgs
    {
        internal MouseDragEventArgs(MouseEventArgs e, DragState state)
            : base(e.X, e.Y, e.Keys)
        {
            State = state;
        }

        public DragState State { get; }
    }
}
