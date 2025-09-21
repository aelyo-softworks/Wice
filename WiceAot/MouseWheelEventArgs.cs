namespace Wice;

/// <summary>
/// Event arguments for a legacy mouse wheel input routed through <see cref="Visual.OnMouseWheel(object?, MouseWheelEventArgs)"/>.
/// </summary>
/// <param name="x">Pointer X position relative to the window (DIPs).</param>
/// <param name="y">Pointer Y position relative to the window (DIPs).</param>
/// <param name="vk">Active modifier keys at the time of the wheel event.</param>
/// <param name="delta">
/// Raw wheel delta in Win32 units (multiples of <see cref="Constants.WHEEL_DELTA"/>).
/// Positive values typically indicate scrolling up/away; negative values indicate down/toward.
/// </param>
/// <param name="orientation">Scroll orientation (vertical or horizontal) reported by the source message.</param>
public class MouseWheelEventArgs(int x, int y, POINTER_MOD vk, int delta, Orientation orientation)
    : MouseEventArgs(x, y, vk)
{
    /// <summary>
    /// Normalized number of "detents" for the wheel movement, computed as
    /// <c>delta / <see cref="Constants.WHEEL_DELTA"/></c>.
    /// </summary>
    public int Delta { get; } = delta / Constants.WHEEL_DELTA;

    /// <summary>
    /// Orientation of the wheel scroll (vertical or horizontal).
    /// </summary>
    public Orientation Orientation { get; } = orientation;

    /// <summary>
    /// Returns a string that includes base mouse data plus the wheel delta and orientation, for diagnostics.
    /// </summary>
    public override string ToString() => base.ToString() + ",DE=" + Delta + ",O=" + Orientation;
}
