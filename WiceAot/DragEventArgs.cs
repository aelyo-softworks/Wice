namespace Wice;

/// <summary>
/// Provides event data for legacy mouse-based drag gestures on a <see cref="Visual"/>.
/// </summary>
/// <remarks>
/// Raised by <see cref="Visual.OnMouseDrag(object?, DragEventArgs)"/> while a drag move is in progress.
/// Coordinates are window-relative. For OLE drag/drop, see <see cref="DragDropEventArgs"/>.
/// </remarks>
/// <param name="x">The X coordinate in window space where the drag event occurred.</param>
/// <param name="y">The Y coordinate in window space where the drag event occurred.</param>
/// <param name="keys">The modifier/button state at the time of the event.</param>
/// <param name="state">The current <see cref="DragState"/> associated with the gesture.</param>
/// <param name="sourceEventArgs">
/// Optional source event arguments that triggered this drag notification
/// (e.g., mouse or pointer event args).
/// </param>
public class DragEventArgs(int x, int y, POINTER_MOD keys, DragState state, EventArgs? sourceEventArgs = null)
    : EventArgs
{
    /// <summary>
    /// Gets the modifier and button state associated with this event.
    /// </summary>
    public POINTER_MOD Keys { get; } = keys;

    /// <summary>
    /// Gets the original source event arguments that produced this drag event, if any.
    /// </summary>
    public EventArgs? SourceEventArgs { get; } = sourceEventArgs;

    /// <summary>
    /// Gets the X coordinate in window space.
    /// </summary>
    public int X { get; } = x;

    /// <summary>
    /// Gets the Y coordinate in window space.
    /// </summary>
    public int Y { get; } = y;

    /// <summary>
    /// Gets the current drag state associated with this gesture.
    /// </summary>
    public DragState State { get; } = state;
}
