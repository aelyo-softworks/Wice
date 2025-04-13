namespace Wice;

public class DragDropSourceEventArgs(DragDropSourceEventType type) : EventArgs
{
    public DragDropSourceEventType Type { get; } = type;
}
