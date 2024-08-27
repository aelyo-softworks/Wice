namespace Wice;

public interface IClickable
{
    event EventHandler<EventArgs>? Click;
}
