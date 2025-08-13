namespace Wice;

public class WindowMessageEventArgs(HWND handle, uint message, WPARAM wParam, LPARAM lParam) : HandledEventArgs
{
    public HWND Handle { get; } = handle;
    public uint Message { get; } = message;
    public WPARAM WParam { get; } = wParam;
    public LPARAM LParam { get; } = lParam;
    public virtual LRESULT Result { get; set; }

    public override string ToString() => WiceCommons.DecodeMessage(Handle, Message, WParam, LParam);
}
