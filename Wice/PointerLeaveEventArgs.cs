namespace Wice
{
    public class PointerLeaveEventArgs : PointerPositionEventArgs
    {
        internal PointerLeaveEventArgs(int pointerId, int x, int y)
            : base(pointerId, x, y)
        {
        }
    }
}
