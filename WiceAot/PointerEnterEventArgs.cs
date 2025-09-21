namespace Wice;

/// <summary>
/// Event data for when a pointing device enters the window or a visual's bounds.
/// </summary>
public class PointerEnterEventArgs(uint pointerId, int x, int y, POINTER_MESSAGE_FLAGS flags)
    : PointerUpdateEventArgs(pointerId, x, y, flags)
{
}
