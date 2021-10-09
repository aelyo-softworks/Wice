namespace Wice
{
    public class PointerLeaveEventArgs : PointerPositionEventArgs
    {
        public PointerLeaveEventArgs(int pointerId, int x, int y)
            : base(pointerId, x, y)
        {
        }
    }
}
