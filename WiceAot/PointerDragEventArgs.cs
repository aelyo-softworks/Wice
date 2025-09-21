namespace Wice;

/// <summary>
/// Provides event data for a pointer drag gesture, combining the latest pointer update
/// information with the current <see cref="DragState"/> describing the gesture progression.
/// </summary>
public class PointerDragEventArgs : PointerUpdateEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PointerDragEventArgs"/> class using values from
    /// a source <see cref="PointerUpdateEventArgs"/> and the current <see cref="DragState"/>.
    /// </summary>
    /// <param name="e">The source pointer update event providing pointer id, position, and flags.</param>
    /// <param name="state">The current drag state for the active gesture.</param>
    public PointerDragEventArgs(PointerUpdateEventArgs e, DragState state)
        : base((e?.PointerId).GetValueOrDefault(), (e?.X).GetValueOrDefault(), (e?.Y).GetValueOrDefault(), (e?.Flags).GetValueOrDefault())
    {
        ExceptionExtensions.ThrowIfNull(e, nameof(e));
        State = state;
    }

    /// <summary>
    /// Gets the current drag state associated with this pointer drag event.
    /// </summary>
    public DragState State { get; }
}
