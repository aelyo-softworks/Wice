namespace Wice;

/// <summary>
/// Provides event data for drag-and-drop source lifecycle events.
/// </summary>
/// <param name="type">The drag-and-drop source event type.</param>
/// <remarks>
/// Use <see cref="Type"/> to determine whether the drag source operation has started or stopped.
/// </remarks>
public class DragDropSourceEventArgs(DragDropSourceEventType type)
    : EventArgs
{
    /// <summary>
    /// Gets the drag-and-drop source event type that triggered this event.
    /// </summary>
    public DragDropSourceEventType Type { get; } = type;
}
