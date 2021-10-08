using DirectN;

namespace Wice
{
    public class MouseButtonEventArgs : MouseEventArgs
    {
        internal MouseButtonEventArgs(int x, int y, POINTER_MOD vk, MouseButton button)
            : base(x, y, vk)
        {
            Button = button;
        }

        public MouseButton Button { get; }
    }
}
