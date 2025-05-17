#nullable enable
namespace DirectN;

[Flags]
public enum WNDCLASS_STYLES : uint
{
    CS_VREDRAW = 1,
    CS_HREDRAW = 2,
    CS_DBLCLKS = 8,
    CS_OWNDC = 32,
    CS_CLASSDC = 64,
    CS_PARENTDC = 128,
    CS_NOCLOSE = 512,
    CS_SAVEBITS = 2048,
    CS_BYTEALIGNCLIENT = 4096,
    CS_BYTEALIGNWINDOW = 8192,
    CS_GLOBALCLASS = 16384,
    CS_IME = 65536,
    CS_DROPSHADOW = 131072,
}
