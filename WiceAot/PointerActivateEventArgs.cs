namespace Wice;

public class PointerActivateEventArgs : PointerEventArgs
{
    public PointerActivateEventArgs(uint pointerId, HWND windowBeingActivated, HT hitTest)
        : base(pointerId)
    {
        WindowBeingActivated = windowBeingActivated;
        HitTest = hitTest;
    }

    public HWND WindowBeingActivated { get; }
    public HT HitTest { get; }

    public override string ToString() => base.ToString() + ",W=" + WindowBeingActivated + ",HT=" + HitTest;
}
