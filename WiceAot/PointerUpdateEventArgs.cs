namespace Wice;

/// <summary>
/// Provides data for pointer update events (movement/hover) including window-relative position
/// and raw WM_POINTER message flags. This is raised when a pointer updates its position over a visual.
/// </summary>
/// <param name="pointerId">System-assigned identifier for the pointer device/contact.</param>
/// <param name="x">Window-relative X coordinate of the pointer.</param>
/// <param name="y">Window-relative Y coordinate of the pointer.</param>
/// <param name="flags">Raw pointer message flags associated with this update.</param>
public class PointerUpdateEventArgs(uint pointerId, int x, int y, POINTER_MESSAGE_FLAGS flags) : PointerPositionEventArgs(pointerId, x, y)
{
    /// <summary>
    /// Gets the raw pointer message flags provided by the OS for this update.
    /// </summary>
    public POINTER_MESSAGE_FLAGS Flags { get; } = flags;

    /// <summary>
    /// Gets a value indicating whether the pointer is within detection range of the digitizer.
    /// </summary>
    public bool IsInRange => Flags.HasFlag(POINTER_MESSAGE_FLAGS.POINTER_MESSAGE_FLAG_INRANGE);

    /// <summary>
    /// Gets a value indicating whether the pointer is in contact (e.g., pen tip down or touch down).
    /// </summary>
    public bool IsInContact => Flags.HasFlag(POINTER_MESSAGE_FLAGS.POINTER_MESSAGE_FLAG_INCONTACT);

    /// <summary>
    /// Gets a value indicating whether this pointer is the primary pointer for the input message.
    /// </summary>
    public bool IsPrimary => Flags.HasFlag(POINTER_MESSAGE_FLAGS.POINTER_MESSAGE_FLAG_PRIMARY);

    /// <summary>
    /// Returns a string that represents the current object, including position and flags.
    /// </summary>
    public override string ToString() => base.ToString() + ",Flags=" + Flags;
}
