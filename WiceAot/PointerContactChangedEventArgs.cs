namespace Wice;

public class PointerContactChangedEventArgs(uint pointerId, int x, int y, POINTER_MESSAGE_FLAGS flags, bool up) : PointerUpdateEventArgs(pointerId, x, y, flags)
{
    public bool IsUp { get; } = up;
    public bool IsDown => !IsUp;
    public bool IsDoubleClick { get; internal set; }

    public override string ToString() => base.ToString() + ",UP=" + IsUp;
}
