namespace Wice
{
    public class MouseButtonEventArgs : MouseEventArgs
    {
        internal MouseButtonEventArgs(int x, int y, MouseVirtualKeys vk, MouseButton button)
            : base(x, y, vk)
        {
            Button = button;
        }

        public MouseButton Button { get; }
    }
}
