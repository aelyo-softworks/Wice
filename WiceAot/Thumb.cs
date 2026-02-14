namespace Wice;

/// <summary>
/// Draggable handle visual that raises drag lifecycle events when the user performs a left-button drag.
/// Based on a <see cref="RoundedRectangle"/> shape.
/// </summary>
public partial class Thumb : RoundedRectangle, IThumb
{
    /// <summary>
    /// Occurs when a drag gesture is initiated (left mouse button pressed and drag move started).
    /// </summary>
    public event EventHandler<DragEventArgs>? DragStarted;

    /// <summary>
    /// Occurs repeatedly while the mouse is dragged with the left button held down.
    /// </summary>
    public event EventHandler<DragEventArgs>? DragDelta;

    /// <summary>
    /// Occurs when a drag gesture completes or is canceled.
    /// </summary>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override void OnMouseDrag(object? sender, DragEventArgs e)
    {
        ExceptionExtensions.ThrowIfNull(e, nameof(e));
        OnDragDelta(sender, e);
        base.OnMouseDrag(sender, e);
    }

    /// <inheritdoc/>
    protected internal override DragState? CancelDragMove(EventArgs e)
    {
        ExceptionExtensions.ThrowIfNull(e, nameof(e));
        var state = base.CancelDragMove(e);
        OnDragCompleted(this, e);
        return state;
    }
}
