namespace Wice.Interop;

public struct WNDCLASSW
{
    public WNDCLASS_STYLES style;
    public nint lpfnWndProc;
    public int cbClsExtra;
    public int cbWndExtra;
    public HINSTANCE hInstance;
    public HICON hIcon;
    public HCURSOR hCursor;
    public HBRUSH hbrBackground;
    public PWSTR lpszMenuName;
    public PWSTR lpszClassName;
}
