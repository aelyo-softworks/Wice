#pragma warning disable CS1591
namespace Wice.Interop;

public struct IMECHARPOSITION
{
    public uint dwSize;
    public uint dwCharPos;
    public POINT pt;
    public uint cLineHeight;
    public RECT rcDocument;
}
