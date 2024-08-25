namespace Wice;

public class MouseButtonEventArgs(int x, int y, POINTER_MOD vk, MouseButton button) : MouseEventArgs(x, y, vk)
{
    public MouseButton Button { get; } = button;
}
