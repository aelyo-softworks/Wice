using DirectN;

namespace Wice
{
    public class MouseWheelEventArgs : MouseEventArgs
    {
        internal MouseWheelEventArgs(int x, int y, MouseVirtualKeys vk, int delta)
            : base(x, y, vk)
        {
            Delta = delta / MessageDecoder.WHEEL_DELTA;
        }

        public int Delta { get; }

        public override string ToString() => base.ToString() + ",DE=" + Delta;
    }
}
