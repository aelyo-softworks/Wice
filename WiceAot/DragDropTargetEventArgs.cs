namespace Wice;

/// <summary>
/// Provides event data for drag-and-drop target notifications.
/// </summary>
/// <param name="type">
/// The kind of drag-and-drop target event being reported (for example, <see cref="DragDropTargetEventType.Enter"/> or <see cref="DragDropTargetEventType.Leave"/>).
/// </param>
/// <param name="hwnd">
/// The handle of the native window that is the target of the drag-and-drop operation.
/// </param>
public class DragDropTargetEventArgs(DragDropTargetEventType type, HWND hwnd)
    : EventArgs
{
    /// <summary>
    /// Gets the drag-and-drop target event type.
    /// </summary>
    public DragDropTargetEventType Type { get; } = type;

    /// <summary>
    /// Gets the handle of the target window associated with the event.
    /// </summary>
    public HWND Hwnd { get; } = hwnd;
}
