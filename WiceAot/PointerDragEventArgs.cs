namespace Wice;

public class PointerDragEventArgs : PointerUpdateEventArgs
{
    public PointerDragEventArgs(PointerUpdateEventArgs e, DragState state)
        : base((e?.PointerId).GetValueOrDefault(), (e?.X).GetValueOrDefault(), (e?.Y).GetValueOrDefault(), (e?.Flags).GetValueOrDefault())
    {
        ArgumentNullException.ThrowIfNull(e);
        State = state;
    }

    public DragState State { get; }
}
