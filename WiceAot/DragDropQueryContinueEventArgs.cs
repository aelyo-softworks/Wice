namespace Wice;

public class DragDropQueryContinueEventArgs(bool escapedPressed, MODIFIERKEYS_FLAGS keyFlags) : EventArgs
{
    public bool EscapedPressed { get; } = escapedPressed;
    public MODIFIERKEYS_FLAGS KeyFlags { get; } = keyFlags;
    public HRESULT Result { get; set; }
}
