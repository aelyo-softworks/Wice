namespace Wice;

public class MouseButtonEventArgs(int x, int y, POINTER_MOD vk, MouseButton button) : MouseEventArgs(x, y, vk)
{
    public MouseButton Button { get; } = button;
    public virtual uint RepeatDelay { get; set; } // if > 0, for mouse button down events only
    public virtual uint RepeatInterval { get; set; } // if > 0, for mouse button down events only
}
