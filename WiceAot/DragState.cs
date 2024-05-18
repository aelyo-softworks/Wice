namespace Wice;

public class DragState
{
    public DragState(Visual visual, MouseButtonEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(visual);
        ArgumentNullException.ThrowIfNull(e);
        StartX = e.X;
        StartY = e.Y;
        Button = e.Button;
    }

    public int StartX { get; }
    public int StartY { get; }
    public int DeltaX { get; internal set; }
    public int DeltaY { get; internal set; }
    public MouseButton Button { get; }

    public override string ToString() => Button + " Start: " + StartX + " x " + StartY + " Delta: " + DeltaX + " x " + DeltaY;
}
