namespace Wice;

public class DragState
{
    public DragState(Visual visual, MouseButtonEventArgs e)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        ExceptionExtensions.ThrowIfNull(e, nameof(e));
        StartX = e.X;
        StartY = e.Y;
        Button = e.Button;
    }

    public int StartX { get; }
    public int StartY { get; }
    public MouseButton Button { get; }
    public virtual int DeltaX { get; set; }
    public virtual int DeltaY { get; set; }

    public override string ToString() => Button + " Start: " + StartX + " x " + StartY + " Delta: " + DeltaX + " x " + DeltaY;
}
