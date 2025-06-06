﻿namespace Wice;

public partial class Thumb : RoundedRectangle
{
    public event EventHandler<DragEventArgs>? DragStarted;
    public event EventHandler<DragEventArgs>? DragDelta;
    public event EventHandler<EventArgs>? DragCompleted;

    protected virtual void OnDragStarted(object? sender, DragEventArgs e) => DragStarted?.Invoke(sender, e);
    protected virtual void OnDragDelta(object? sender, DragEventArgs e) => DragDelta?.Invoke(sender, e);
    protected virtual void OnDragCompleted(object? sender, EventArgs e) => DragCompleted?.Invoke(sender, e);

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

    public void CancelDrag(EventArgs? e = null) => CancelDragMove(e ?? EventArgs.Empty);

    protected override void OnMouseDrag(object? sender, DragEventArgs e)
    {
        ExceptionExtensions.ThrowIfNull(e, nameof(e));
        OnDragDelta(sender, e);
        base.OnMouseDrag(sender, e);
    }

    protected override DragState? CancelDragMove(EventArgs e)
    {
        ExceptionExtensions.ThrowIfNull(e, nameof(e));
        var state = base.CancelDragMove(e);
        OnDragCompleted(this, e);
        return state;
    }
}
