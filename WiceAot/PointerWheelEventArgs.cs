namespace Wice;

public class PointerWheelEventArgs(uint pointerId, int x, int y, int delta, Orientation orientation) : PointerPositionEventArgs(pointerId, x, y)
{
    public int Delta { get; } = delta / Constants.WHEEL_DELTA;
    public Orientation Orientation { get; } = orientation;

    public override string ToString() => base.ToString() + ",DE=" + Delta + ",O=" + Orientation;
}
