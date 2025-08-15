namespace Wice;

/// <summary>
/// Event data for when a pointing device enters the window or a visual's bounds.
/// </summary>
/// <remarks>
/// - This is typically raised when the pointer transitions from outside to inside the window/client area
///   or a specific <see cref="Visual"/>.
/// - Coordinates <see cref="PointerPositionEventArgs.X"/> and <see cref="PointerPositionEventArgs.Y"/> are window-relative.
///   Use <see cref="PointerPositionEventArgs.GetPosition(Visual)"/> to obtain coordinates in a visual's space.
/// - Additional state is available via <see cref="PointerUpdateEventArgs.Flags"/> sourced from <see cref="POINTER_MESSAGE_FLAGS"/>.
/// </remarks>
/// <param name="pointerId">System-assigned identifier of the pointer raising the event.</param>
/// <param name="x">Window-relative X coordinate at the time the pointer enters.</param>
/// <param name="y">Window-relative Y coordinate at the time the pointer enters.</param>
/// <param name="flags">Bitwise combination of <see cref="POINTER_MESSAGE_FLAGS"/> describing the pointer state.</param>
/// <example>
/// Typical usage in an event handler:
/// <code>
/// void OnPointerEnter(object sender, PointerEnterEventArgs e)
/// {
///     // Convert to a specific visual's coordinate space if needed
///     // var pt = e.GetPosition(myVisual);
///
///     // Inspect flags/state from the base type
///     // if (e.IsInRange && e.IsPrimary) { /* ... */ }
/// }
/// </code>
/// </example>
/// <seealso cref="PointerUpdateEventArgs"/>
/// <seealso cref="PointerPositionEventArgs"/>
/// <seealso cref="POINTER_MESSAGE_FLAGS"/>
/// <seealso cref="Visual"/>
public class PointerEnterEventArgs(uint pointerId, int x, int y, POINTER_MESSAGE_FLAGS flags)
    : PointerUpdateEventArgs(pointerId, x, y, flags)
{
}
