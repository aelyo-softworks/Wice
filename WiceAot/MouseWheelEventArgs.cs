using DirectN;

namespace Wice
{
    public class MouseWheelEventArgs : MouseEventArgs
    {
        public MouseWheelEventArgs(int x, int y, POINTER_MOD vk, int delta, Orientation orientation)
            : base(x, y, vk)
        {
            Delta = delta / Constants.WHEEL_DELTA;
            Orientation = orientation;
        }

        public int Delta { get; }
        public Orientation Orientation { get; }

        public override string ToString() => base.ToString() + ",DE=" + Delta + ",O=" + Orientation;
    }
}
