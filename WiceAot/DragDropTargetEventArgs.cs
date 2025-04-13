namespace Wice;

public class DragDropTargetEventArgs(DragDropTargetEventType type, HWND hwnd) : EventArgs
{
    public DragDropTargetEventType Type { get; } = type;
    public HWND Hwnd { get; } = hwnd;
}
