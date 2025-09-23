namespace Wice;

/// <summary>
/// Event arguments for pointer wheel scrolling (mouse wheel, touchpad scroll, pen barrel rotation).
/// </summary>
/// <param name="pointerId">Unique identifier of the source pointer.</param>
/// <param name="x">X coordinate of the pointer relative to the window.</param>
/// <param name="y">Y coordinate of the pointer relative to the window.</param>
/// <param name="delta">
/// Raw wheel delta reported by the platform (typically a multiple of <see cref="Constants.WHEEL_DELTA"/>; commonly 120).
/// Positive values indicate forward/up scroll; negative values indicate backward/down scroll.
/// </param>
/// <param name="orientation">
/// Axis of the wheel movement for devices that support horizontal scrolling as well as vertical.
/// </param>
public class PointerWheelEventArgs(uint pointerId, int x, int y, int delta, Orientation orientation) : PointerPositionEventArgs(pointerId, x, y)
{
    /// <summary>
    /// Gets the normalized wheel delta expressed in detents (raw <c>delta</c> divided by <see cref="Constants.WHEEL_DELTA"/>).
    /// Positive values indicate scroll up/forward; negative values indicate scroll down/backward.
    /// </summary>
    public int Delta { get; } = delta / Constants.WHEEL_DELTA;

    /// <summary>
    /// Gets the orientation of the wheel movement (vertical or horizontal).
    /// </summary>
    public Orientation Orientation { get; } = orientation;

    /// <inheritdoc/>
    public override string ToString() => base.ToString() + ",DE=" + Delta + ",O=" + Orientation;
}
