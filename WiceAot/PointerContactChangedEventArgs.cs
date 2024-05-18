namespace Wice;

public class PointerContactChangedEventArgs : PointerUpdateEventArgs
{
    public PointerContactChangedEventArgs(uint pointerId, int x, int y, POINTER_MESSAGE_FLAGS flags, bool up)
        : base(pointerId, x, y, flags)
    {
        IsUp = up;
    }

    public bool IsUp { get; }
    public bool IsDown => !IsUp;
    public bool IsDoubleClick { get; internal set; }

    public override string ToString() => base.ToString() + ",UP=" + IsUp;
}
