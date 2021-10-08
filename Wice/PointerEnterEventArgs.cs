namespace Wice
{
    public class PointerEnterEventArgs : PointerPositionEventArgs
    {
        internal PointerEnterEventArgs(int pointerId, int x, int y)
            : base(pointerId, x, y)
        {
        }
    }
}
