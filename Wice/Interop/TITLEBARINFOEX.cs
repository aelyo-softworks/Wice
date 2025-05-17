namespace Wice.Interop;

public struct TITLEBARINFOEX
{
    public uint cbSize;
    public RECT rcTitleBar;
    public STATE_SYSTEM rgstateTitleBar;
    public STATE_SYSTEM rgstateReserved;
    public STATE_SYSTEM rgstateMinimizeButton;
    public STATE_SYSTEM rgstateMaximizeButton;
    public STATE_SYSTEM rgstateHelpButton;
    public STATE_SYSTEM rgstateCloseButton;
    public RECT rgrectTitleBar;
    public RECT rgrectReserved;
    public RECT rgrectMinimizeButton;
    public RECT rgrectMaximizeButton;
    public RECT rgrectHelpButton;
    public RECT rgrectCloseButton;
}
