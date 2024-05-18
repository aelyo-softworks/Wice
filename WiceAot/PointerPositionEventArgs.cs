namespace Wice;

public abstract class PointerPositionEventArgs(uint pointerId, int x, int y) : PointerEventArgs(pointerId)
{
    internal readonly List<Visual> _visualsStack = [];

    // window relative
    public int X { get; } = x;
    public int Y { get; } = y;
    public IReadOnlyList<Visual> VisualsStack => _visualsStack;

    public POINT GetPosition(Visual visual)
    {
        ArgumentNullException.ThrowIfNull(visual);
        return visual.GetRelativePosition(X, Y);
    }

    public bool Hits(Visual visual)
    {
        ArgumentNullException.ThrowIfNull(visual);
        var size = visual.RenderSize;
        if (size.IsInvalid)
            return false;

        return new D2D_RECT_F(size).Contains(GetPosition(visual));
    }

    public override string ToString() => base.ToString() + ",X=" + X + ",Y=" + Y;
}
