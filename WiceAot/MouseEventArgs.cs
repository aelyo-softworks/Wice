namespace Wice;

public class MouseEventArgs : HandledEventArgs
{
    internal readonly List<Visual> _visualsStack = new();

    public MouseEventArgs(int x, int y, POINTER_MOD vk)
    {
        X = x;
        Y = y;
        Keys = vk;
    }

    public POINTER_MOD Keys { get; }
    public PointerEventArgs SourcePointerEvent { get; internal set; } // will be null if EnableMouseInPointer was not called

    // window relative
    public int X { get; }
    public int Y { get; }

    public IReadOnlyList<Visual> VisualsStack
    {
        get
        {
            if (!(SourcePointerEvent is PointerPositionEventArgs evt))
                return _visualsStack;

            return evt._visualsStack;
        }
    }

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

    public override string ToString() => "X=" + X + ",Y=" + Y + ",VK=" + Keys;
}
