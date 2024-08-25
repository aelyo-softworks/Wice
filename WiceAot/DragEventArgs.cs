namespace Wice;

public class DragEventArgs(int x, int y, POINTER_MOD keys, DragState state, EventArgs? sourceEventArgs = null) : EventArgs
{
    public POINTER_MOD Keys { get; } = keys;
    public EventArgs SourceEventArgs { get; } = sourceEventArgs;

    // window relative
    public int X { get; } = x;
    public int Y { get; } = y;

    public DragState State { get; } = state;
}
