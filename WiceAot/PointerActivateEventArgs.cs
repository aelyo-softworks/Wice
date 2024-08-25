namespace Wice;

public class PointerActivateEventArgs(uint pointerId, HWND windowBeingActivated, HT hitTest) : PointerEventArgs(pointerId)
{
    public HWND WindowBeingActivated { get; } = windowBeingActivated;
    public HT HitTest { get; } = hitTest;

    public override string ToString() => base.ToString() + ",W=" + WindowBeingActivated + ",HT=" + HitTest;
}
