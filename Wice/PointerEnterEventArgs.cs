namespace Wice
{
    public class PointerEnterEventArgs : PointerPositionEventArgs
    {
        public PointerEnterEventArgs(int pointerId, int x, int y)
            : base(pointerId, x, y)
        {
        }
    }
}
