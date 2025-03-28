namespace Wice
{
    public class MouseWheelEventArgs(int x, int y, POINTER_MOD vk, int delta, Orientation orientation) : MouseEventArgs(x, y, vk)
    {
        public int Delta { get; } = delta / Constants.WHEEL_DELTA;
        public Orientation Orientation { get; } = orientation;

        public override string ToString() => base.ToString() + ",DE=" + Delta + ",O=" + Orientation;
    }
}
