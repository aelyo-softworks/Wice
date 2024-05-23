using Windows.UI;

namespace Wice.Utilities;

public static class Extensions
{
    public static Size ToSize(this D2D_SIZE_F size) => new(size.width, size.height);
    public static Size ToSize(this SIZE size) => new(size.cx, size.cy);
    public static Color ToColor(this D3DCOLORVALUE value) => Color.FromArgb(value.BA, value.BR, value.BG, value.BB);
    public static D3DCOLORVALUE ToColor(this Color value) => D3DCOLORVALUE.FromArgb(value.A, value.R, value.G, value.B);
}
