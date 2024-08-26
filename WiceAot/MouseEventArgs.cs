namespace Wice;

public class MouseEventArgs(int x, int y, POINTER_MOD vk) : HandledEventArgs
{
    internal readonly List<Visual> _visualsStack = [];

    public POINTER_MOD Keys { get; } = vk;
    public PointerEventArgs? SourcePointerEvent { get; internal set; } // will be null if EnableMouseInPointer was not called

    // window relative
    public int X { get; } = x;
    public int Y { get; } = y;

    public IReadOnlyList<Visual> VisualsStack
    {
        get
        {
            if (SourcePointerEvent is not PointerPositionEventArgs evt)
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
