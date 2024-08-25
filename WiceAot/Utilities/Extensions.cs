namespace Wice.Utilities;

public static class Extensions
{
    public static Size ToSize(this D2D_SIZE_F size) => new(size.width, size.height);
    public static Size ToSize(this SIZE size) => new(size.cx, size.cy);
    public static Color ToColor(this D3DCOLORVALUE value) => Color.FromArgb(value.BA, value.BR, value.BG, value.BB);
    public static D3DCOLORVALUE ToColor(this Color value) => D3DCOLORVALUE.FromArgb(value.A, value.R, value.G, value.B);
    public static D2D_RECT_F ToRound(this D2D_RECT_F rect) => new(rect.left.Round(), rect.top.Round(), rect.right.Round(), rect.bottom.Round());
    public static D2D_POINT_2F ToD2D_POINT_2F(this POINT pt) => new(pt.x, pt.y);
    public static bool Is19H1OrHigher => WindowsVersionUtilities.IsApiContractAvailable(8);

    public static Size GetWinSize(this IComObject<IWICBitmapSource> bitmap) => GetWinSize(bitmap?.Object!);
    public static Size GetWinSize(this IWICBitmapSource bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        bitmap.GetSize(out var width, out var height);
        return new Size(width, height);
    }
}
