namespace Wice;

/// <summary>
/// Defines the contract for a thumb visual that supports dragging operations.
/// </summary>
public interface IThumb
{
    /// <summary>
    /// Occurs when a drag gesture is initiated (left mouse button pressed and drag move started).
    /// </summary>
    event EventHandler<DragEventArgs>? DragStarted;

    /// <summary>
    /// Occurs repeatedly while the mouse is dragged with the left button held down.
    /// </summary>
    event EventHandler<DragEventArgs>? DragDelta;

    /// <summary>
    /// Occurs when a drag gesture completes or is canceled.
    /// </summary>
    event EventHandler<EventArgs>? DragCompleted;

    /// <summary>
    /// Cancels the current drag operation and restores the UI to its state before the drag began.
    /// </summary>
    void CancelDrag(EventArgs? e = null);
}
