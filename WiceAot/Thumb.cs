namespace Wice;

/// <summary>
/// Draggable handle visual that raises drag lifecycle events when the user performs a left-button drag.
/// Inherits from <see cref="RoundedRectangle"/> and uses the base <see cref="Visual"/> drag infrastructure
/// (<see cref="Visual.DragMove(MouseButtonEventArgs)"/> / <see cref="Visual.CancelDragMove(System.EventArgs)"/>).
/// </summary>
/// <remarks>
/// Behavior:
/// - On left mouse down, starts a drag gesture via <see cref="Visual.DragMove(MouseButtonEventArgs)"/> and raises <see cref="DragStarted"/>.
/// - During drag, forwards base drag updates to <see cref="DragDelta"/>.
/// - When the drag is canceled or completes, raises <see cref="DragCompleted"/>.
/// </remarks>
public partial class Thumb : RoundedRectangle
{
    /// <summary>
    /// Occurs when a drag gesture is initiated (left mouse button pressed and drag move started).
    /// </summary>
    /// <remarks>
    /// Raised after <see cref="Visual.DragMove(MouseButtonEventArgs)"/> returns the initial <see cref="Wice.DragState"/>.
    /// </remarks>
    public event EventHandler<DragEventArgs>? DragStarted;

    /// <summary>
    /// Occurs repeatedly while the mouse is dragged with the left button held down.
    /// </summary>
    /// <remarks>
    /// This is raised from <see cref="OnMouseDrag(object?, DragEventArgs)"/>, which is called by the base input pipeline.
    /// </remarks>
    public event EventHandler<DragEventArgs>? DragDelta;

    /// <summary>
    /// Occurs when a drag gesture completes or is canceled.
    /// </summary>
    /// <remarks>
    /// Raised from <see cref="CancelDragMove(System.EventArgs)"/> after the base implementation releases capture and finalizes drag state.
    /// </remarks>
    public event EventHandler<EventArgs>? DragCompleted;

    /// <summary>
    /// Raises <see cref="DragStarted"/> allowing derived classes to customize the event flow.
    /// </summary>
    /// <param name="sender">The event sender, typically <c>this</c>.</param>
    /// <param name="e">The drag event data.</param>
    protected virtual void OnDragStarted(object? sender, DragEventArgs e) => DragStarted?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="DragDelta"/> allowing derived classes to customize the event flow.
    /// </summary>
    /// <param name="sender">The event sender, typically <c>this</c>.</param>
    /// <param name="e">The drag event data describing the current displacement.</param>
    protected virtual void OnDragDelta(object? sender, DragEventArgs e) => DragDelta?.Invoke(sender, e);

    /// <summary>
    /// Raises <see cref="DragCompleted"/> allowing derived classes to customize the event flow.
    /// </summary>
    /// <param name="sender">The event sender, typically <c>this</c>.</param>
    /// <param name="e">Event args associated with the completion/cancellation.</param>
    protected virtual void OnDragCompleted(object? sender, EventArgs e) => DragCompleted?.Invoke(sender, e);

    /// <summary>
    /// Handles mouse button down input. Starts a drag move when the left button is pressed and raises <see cref="DragStarted"/>.
    /// Always marks the event as handled.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Mouse button event arguments.</param>
    /// <remarks>
    /// Calls <see cref="Visual.DragMove(MouseButtonEventArgs)"/> to capture the mouse and create a <see cref="Wice.DragState"/>.
    /// The base implementation is called after starting the drag and raising <see cref="DragStarted"/>.
    /// </remarks>
    protected override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
    {
        ExceptionExtensions.ThrowIfNull(e, nameof(e));

        e.Handled = true;
        if (e.Button == MouseButton.Left)
        {
            var state = DragMove(e);
            OnDragStarted(this, new DragEventArgs(e.X, e.X, e.Keys, state, e));
        }
        base.OnMouseButtonDown(sender, e);
    }

    /// <summary>
    /// Cancels the current drag gesture, if any, and raises <see cref="DragCompleted"/>.
    /// </summary>
    /// <param name="e">Optional event args; when null, <see cref="System.EventArgs.Empty"/> is used.</param>
    public void CancelDrag(EventArgs? e = null) => CancelDragMove(e ?? EventArgs.Empty);

    /// <summary>
    /// Forwards drag updates to <see cref="DragDelta"/> during an active drag gesture.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Drag delta event data.</param>
    /// <remarks>
    /// Invokes <see cref="OnDragDelta(object?, DragEventArgs)"/> and then calls the base implementation.
    /// </remarks>
    protected override void OnMouseDrag(object? sender, DragEventArgs e)
    {
        ExceptionExtensions.ThrowIfNull(e, nameof(e));
        OnDragDelta(sender, e);
        base.OnMouseDrag(sender, e);
    }

    /// <summary>
    /// Cancels the current drag move operation and raises <see cref="DragCompleted"/>.
    /// </summary>
    /// <param name="e">Event args associated with the completion/cancellation.</param>
    /// <returns>
    /// The prior <see cref="Wice.DragState"/> if a drag was active; otherwise <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Calls the base <see cref="Visual.CancelDragMove(System.EventArgs)"/> to release capture and finalize drag state,
    /// then raises <see cref="DragCompleted"/>.
    /// </remarks>
    protected override DragState? CancelDragMove(EventArgs e)
    {
        ExceptionExtensions.ThrowIfNull(e, nameof(e));
        var state = base.CancelDragMove(e);
        OnDragCompleted(this, e);
        return state;
    }
}
