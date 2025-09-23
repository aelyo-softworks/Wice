namespace Wice;

/// <summary>
/// Provides event data for pointer contact state changes (press/release).
/// Raised when a pointer transitions between in-contact (down) and not-in-contact (up),
/// carrying the window-relative position and the raw WM_POINTER flags at the time of the transition.
/// </summary>
/// <param name="pointerId">System-assigned identifier for the pointer device/contact.</param>
/// <param name="x">Window-relative X coordinate of the pointer at the time of the contact change.</param>
/// <param name="y">Window-relative Y coordinate of the pointer at the time of the contact change.</param>
/// <param name="flags">Raw pointer message flags associated with this contact change.</param>
/// <param name="up">True when the contact transitioned to the released state; false when it transitioned to pressed.</param>
public class PointerContactChangedEventArgs(uint pointerId, int x, int y, POINTER_MESSAGE_FLAGS flags, bool up)
    : PointerUpdateEventArgs(pointerId, x, y, flags)
{
    /// <summary>
    /// Gets a value indicating whether the pointer contact transitioned to the released state.
    /// </summary>
    public bool IsUp { get; } = up;

    /// <summary>
    /// Gets a value indicating whether the pointer contact transitioned to the pressed state.
    /// </summary>
    public bool IsDown => !IsUp;

    /// <summary>
    /// Gets a value indicating whether this contact change is considered a double-click.
    /// This is set internally by the input pipeline based on timing and hit-test heuristics.
    /// </summary>
    public bool IsDoubleClick { get; internal set; }

    /// <inheritdoc/>
    public override string ToString() => base.ToString() + ",UP=" + IsUp;
}
