namespace Wice;

public class PointerLeaveEventArgs(uint pointerId, int x, int y, POINTER_MESSAGE_FLAGS flags) : PointerUpdateEventArgs(pointerId, x, y, flags)
{
}
