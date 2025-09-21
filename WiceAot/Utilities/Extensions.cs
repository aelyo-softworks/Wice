namespace Wice.Utilities;

/// <summary>
/// Utility extensions for interop conversions and helpers across Win32, Direct2D/DirectWrite, and WIC types.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Converts a Direct2D size (D2D_SIZE_F) to a managed <see cref="Size"/>.
    /// </summary>
    /// <param name="size">The source size with floating-point components.</param>
    /// <returns>A <see cref="Size"/> using the width and height from <paramref name="size"/>.</returns>
    public static Size ToSize(this D2D_SIZE_F size) => new(size.width, size.height);

    /// <summary>
    /// Converts a Win32 <see cref="SIZE"/> to a managed <see cref="Size"/>.
    /// </summary>
    /// <param name="size">The Win32 SIZE structure.</param>
    /// <returns>A <see cref="Size"/> with the same dimensions.</returns>
    public static Size ToSize(this SIZE size) => new(size.cx, size.cy);

    /// <summary>
    /// Converts a Direct3D color value to a managed <see cref="Color"/>.
    /// </summary>
    /// <param name="value">The source <see cref="D3DCOLORVALUE"/>.</param>
    /// <returns>A <see cref="Color"/> built from the ARGB components contained in <paramref name="value"/>.</returns>
    public static Color ToColor(this D3DCOLORVALUE value) => Color.FromArgb(value.BA, value.BR, value.BG, value.BB);

    /// <summary>
    /// Converts a managed <see cref="Color"/> to a Direct3D <see cref="D3DCOLORVALUE"/>.
    /// </summary>
    /// <param name="value">The source color.</param>
    /// <returns>A <see cref="D3DCOLORVALUE"/> built from the ARGB components of <paramref name="value"/>.</returns>
    public static D3DCOLORVALUE ToColor(this Color value) => D3DCOLORVALUE.FromArgb(value.A, value.R, value.G, value.B);

    /// <summary>
    /// Rounds the edges of a Direct2D rectangle to the nearest integral values.
    /// </summary>
    /// <param name="rect">The source rectangle with floating-point coordinates.</param>
    /// <returns>A new <see cref="D2D_RECT_F"/> whose coordinates are rounded.</returns>
    public static D2D_RECT_F ToRound(this D2D_RECT_F rect) => new(rect.left.Round(), rect.top.Round(), rect.right.Round(), rect.bottom.Round());

    /// <summary>
    /// Converts a Win32 <see cref="POINT"/> to a Direct2D <see cref="D2D_POINT_2F"/>.
    /// </summary>
    /// <param name="pt">The Win32 point.</param>
    /// <returns>A <see cref="D2D_POINT_2F"/> with the same coordinates.</returns>
    public static D2D_POINT_2F ToD2D_POINT_2F(this POINT pt) => new(pt.x, pt.y);

    /// <summary>
    /// Gets a value indicating whether the running OS is Windows 10 19H1 (v1903) or higher.
    /// </summary>
    public static bool Is19H1OrHigher => WindowsVersionUtilities.IsApiContractAvailable(8);

    /// <summary>
    /// Gets the size of a WIC bitmap from an <see cref="IComObject{T}"/> wrapper.
    /// </summary>
    /// <param name="bitmap">The COM wrapper around <see cref="IWICBitmapSource"/>.</param>
    /// <returns>The bitmap size as a managed <see cref="Size"/>.</returns>
    public static Size GetWinSize(this IComObject<IWICBitmapSource> bitmap) => GetWinSize(bitmap?.Object!);

    /// <summary>
    /// Gets the size of a WIC bitmap.
    /// </summary>
    /// <param name="bitmap">The WIC bitmap source.</param>
    /// <returns>The bitmap size as a managed <see cref="Size"/>.</returns>
    public static Size GetWinSize(this IWICBitmapSource bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        bitmap.GetSize(out var width, out var height);
        return new Size(width, height);
    }

    /// <summary>
    /// Retrieves a specific attribute from a <see cref="MemberDescriptor"/> if present.
    /// </summary>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <param name="descriptor">The member descriptor.</param>
    /// <returns>The attribute instance if found; otherwise, <see langword="null"/>.</returns>
    public static T? GetAttribute<T>(this MemberDescriptor descriptor) where T : Attribute
    {
        if (descriptor == null)
            return null;

        return GetAttribute<T>(descriptor.Attributes);
    }

    /// <summary>
    /// Retrieves a specific attribute from an <see cref="AttributeCollection"/> if present.
    /// </summary>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <param name="attributes">The attribute collection.</param>
    /// <returns>The attribute instance if found; otherwise, <see langword="null"/>.</returns>
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

    /// <summary>
    /// Determines whether an array is null or empty.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="enumerable">The array to test.</param>
    /// <returns><see langword="true"/> if <paramref name="enumerable"/> is null or has length 0; otherwise, <see langword="false"/>.</returns>
    public static bool IsEmpty<T>(this T[]? enumerable)
    {
        if (enumerable == null)
            return true;

        return enumerable.Length == 0;
    }

    /// <summary>
    /// Returns the most compact underlying integral type for an enum based on its maximum bit width.
    /// </summary>
    /// <param name="enumMaxPower">The maximum number of bits required to represent any enum value (e.g., 8, 16, 32, 64).</param>
    /// <returns>The corresponding underlying integral <see cref="Type"/>.</returns>
    public static Type GetEnumUnderlyingType(int enumMaxPower) => enumMaxPower switch
    {
        8 => typeof(sbyte),
        16 => typeof(short),
        64 => typeof(long),
        _ => typeof(int),
    };

    /// <summary>
    /// Converts a Direct2D 3x2 matrix to a DirectWrite <see cref="DWRITE_MATRIX"/>.
    /// </summary>
    /// <param name="value">The source matrix.</param>
    /// <returns>A <see cref="DWRITE_MATRIX"/> with equivalent components.</returns>
    public static DWRITE_MATRIX ToMatrix(this D2D_MATRIX_3X2_F value) => new()
    {
        m11 = value._11,
        m12 = value._12,
        m21 = value._21,
        m22 = value._22,
        dx = value._31,
        dy = value._32,
    };

    /// <summary>
    /// Converts a <see cref="POINTL"/> to a Win32 <see cref="POINT"/>.
    /// </summary>
    /// <param name="value">The source <see cref="POINTL"/>.</param>
    /// <returns>A <see cref="POINT"/> with the same coordinates.</returns>
    public static POINT ToPOINT(this POINTL value) => new()
    {
        x = value.x,
        y = value.y,
    };

    /// <summary>
    /// Converts a Win32 <see cref="POINT"/> to a <see cref="POINTL"/>.
    /// </summary>
    /// <param name="value">The source <see cref="POINT"/>.</param>
    /// <returns>A <see cref="POINTL"/> with the same coordinates.</returns>
    public static POINTL ToPOINTL(this POINT value) => new()
    {
        x = value.x,
        y = value.y,
    };
}
