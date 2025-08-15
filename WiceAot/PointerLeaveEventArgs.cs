namespace Wice;

/// <summary>
/// Provides data for a pointer leave event, raised when a pointer exits a visual's bounds or the hosting window.
/// </summary>
/// <param name="pointerId">System-assigned identifier for the pointer device/contact that left.</param>
/// <param name="x">Window-relative X coordinate at the time the pointer left.</param>
/// <param name="y">Window-relative Y coordinate at the time the pointer left.</param>
/// <param name="flags">Raw pointer message flags associated with the leave notification.</param>
/// <remarks>
/// Inherits window-space position and WM_POINTER flags from <see cref="PointerUpdateEventArgs"/>.
/// </remarks>
/// <seealso cref="PointerUpdateEventArgs"/>
/// <seealso cref="PointerPositionEventArgs"/>
public class PointerLeaveEventArgs(uint pointerId, int x, int y, POINTER_MESSAGE_FLAGS flags)
    : PointerUpdateEventArgs(pointerId, x, y, flags)
{
}
