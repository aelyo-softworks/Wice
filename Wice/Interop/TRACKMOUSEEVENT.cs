namespace Wice.Interop;

public struct TRACKMOUSEEVENT
{
    public uint cbSize;
    public TRACKMOUSEEVENT_FLAGS dwFlags;
    public HWND hwndTrack;
    public uint dwHoverTime;
}
