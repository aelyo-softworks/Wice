namespace Wice;

/// <summary>
/// Describes the state of a mouse drag gesture for a <see cref="Visual"/>.
/// </summary>
/// <remarks>
/// - Captures the window-relative start position and the pressed mouse button at the time the drag begins.
/// - <see cref="DeltaX"/> and <see cref="DeltaY"/> are updated by the drag handling logic to represent the
///   current displacement from the start position (typically current - start).
/// - Instances are commonly created by <see cref="Visual.CreateDragState(MouseButtonEventArgs)"/> and used during
///   drag operations such as moving or resizing visuals.
/// </remarks>
public class DragState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DragState"/> class using the start position from the mouse event.
    /// </summary>
    /// <param name="visual">The visual initiating the drag gesture.</param>
    /// <param name="e">The mouse button event that started the drag (window-relative coordinates).</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="visual"/> or <paramref name="e"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// The constructor captures <see cref="StartX"/>, <see cref="StartY"/>, and <see cref="Button"/> from <paramref name="e"/>.
    /// </remarks>
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
    /// <remarks>
    /// Typically computed as <c>currentX - StartX</c> by the drag handling logic.
    /// </remarks>
    public virtual int DeltaX { get; set; }

    /// <summary>
    /// Gets or sets the current Y delta from the start position.
    /// </summary>
    /// <remarks>
    /// Typically computed as <c>currentY - StartY</c> by the drag handling logic.
    /// </remarks>
    public virtual int DeltaY { get; set; }

    /// <summary>
    /// Returns a string that represents the current drag state, including button, start point, and deltas.
    /// </summary>
    /// <returns>A human-readable string describing the drag state.</returns>
    public override string ToString() => Button + " Start: " + StartX + " x " + StartY + " Delta: " + DeltaX + " x " + DeltaY;
}
