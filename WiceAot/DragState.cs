namespace Wice;

/// <summary>
/// Describes the state of a mouse drag gesture for a <see cref="Visual"/>.
/// </summary>
public class DragState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DragState"/> class using the start position from the mouse event.
    /// </summary>
    /// <param name="visual">The visual initiating the drag gesture.</param>
    /// <param name="e">The mouse button event that started the drag (window-relative coordinates).</param>
    public DragState(Visual visual, MouseButtonEventArgs e)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        ExceptionExtensions.ThrowIfNull(e, nameof(e));
        StartX = e.X;
        StartY = e.Y;
        Button = e.Button;
    }

    /// <summary>
    /// Gets the window-relative X coordinate where the drag started.
    /// </summary>
    public int StartX { get; }

    /// <summary>
    /// Gets the window-relative Y coordinate where the drag started.
    /// </summary>
    public int StartY { get; }

    /// <summary>
    /// Gets the mouse button that initiated the drag.
    /// </summary>
    public MouseButton Button { get; }

    /// <summary>
    /// Gets or sets the current X delta from the start position.
    /// </summary>
    public virtual int DeltaX { get; set; }

    /// <summary>
    /// Gets or sets the current Y delta from the start position.
    /// </summary>
    public virtual int DeltaY { get; set; }

    /// <summary>
    /// Gets or sets an object that contains data associated with the state.
    /// </summary>
    public virtual object? Tag { get; set; }

    /// <inheritdoc/>
    public override string ToString() => Button + " Start: " + StartX + " x " + StartY + " Delta: " + DeltaX + " x " + DeltaY;
}
