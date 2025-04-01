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

    public static T? GetAttribute<T>(this MemberDescriptor descriptor) where T : Attribute
    {
        if (descriptor == null)
            return null;

        return GetAttribute<T>(descriptor.Attributes);
    }

    public static T? GetAttribute<T>(this AttributeCollection attributes) where T : Attribute
    {
        if (attributes == null)
            return null;

        foreach (var att in attributes)
        {
            if (typeof(T).IsAssignableFrom(att.GetType()))
                return (T)att;
        }
        return null;
    }

    public static bool IsEmpty<T>(this T[]? enumerable)
    {
        if (enumerable == null)
            return true;

        return enumerable.Length == 0;
    }

    public static Type GetEnumUnderlyingType(int enumMaxPower) => enumMaxPower switch
    {
        8 => typeof(sbyte),
        16 => typeof(short),
        64 => typeof(long),
        _ => typeof(int),
    };

    public static DWRITE_MATRIX ToMatrix(this D2D_MATRIX_3X2_F value) => new()
    {
        m11 = value._11,
        m12 = value._12,
        m21 = value._21,
        m22 = value._22,
        dx = value._31,
        dy = value._32,
    };
}
