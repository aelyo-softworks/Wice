namespace Wice.Utilities;

public static class Extensions
{
    public static Size ToSize(this D2D_SIZE_F size) => new(size.width, size.height);
    public static Size ToSize(this SIZE size) => new(size.width, size.height);
    public static D2D_SIZE_F ToD2D_SIZE_F(this SIZE size) => new(size.width, size.height);
    public static RECT ToRECT(this D2D_RECT_F rect) => new(rect.left, rect.top, rect.right, rect.bottom);
    public static Vector2 ToVector2(this SIZE size) => new(size.width, size.height);
    public static D2D_RECT_F ToRound(this D2D_RECT_F rect) => new(rect.left.Round(), rect.top.Round(), rect.right.Round(), rect.bottom.Round());
    public static System.Drawing.Point ToPoint(this POINT pt) => new(pt.x, pt.y);
    public static POINT ToPOINT(this System.Drawing.Point pt) => new(pt.X, pt.Y);
    public static POINT ToPOINT(this D2D_POINT_2F pt) => new(pt.x, pt.y);

    public static Vector2 ToVector2(this D2D_SIZE_F size) => new(size.width, size.height);
    public static Vector3 ToVector3(this D2D_SIZE_F size) => new(size.width, size.height, 0);
    public static int SignedLOWORD(this uint value) => (short)(value & 0xffff);
    public static int SignedHIWORD(this uint value) => (short)((value >> 0x10) & 0xffff);

    public static long ToInt64(this nint value) => ((IntPtr)value).ToInt64(); // not sure why we need that for .NET Framework
    public static uint ToUInt32(this nuint value) => ((UIntPtr)value).ToUInt32(); // not sure why we need that for .NET Framework
    public static uint HIWORD(this uint value) => (value >> 0x10) & 0xffff;
    public static uint HIWORD(this nuint value) => HIWORD((uint)(ulong)value);
    public static uint LOWORD(this uint value) => value & 0xffff;
    public static uint LOWORD(this nuint value) => LOWORD((uint)(ulong)value);
    public static int SignedHIWORD(this nuint value) => SignedHIWORD((int)(long)value);
    public static int SignedHIWORD(this int value) => (short)((value >> 0x10) & 0xffff);

    public static uint GetPointerId(this WPARAM wParam) => wParam.Value.LOWORD();
    public static POINTER_MESSAGE_FLAGS GetPointerFlags(this WPARAM wParam) => (POINTER_MESSAGE_FLAGS)wParam.Value.HIWORD();
    public static int GetWheelDelta(this WPARAM wParam) => (int)wParam.Value.HIWORD();

    public static void SafeDispose(this IDisposable? disposable)
    {
        if (disposable == null)
            return;

        try
        {
            disposable.Dispose();
        }
        catch
        {
            // continue;
        }
    }

    public static bool TryParseD2D_RECT_F(object input, out D2D_RECT_F value) => TryParseD2D_RECT_F(input, null, out value);
    public static bool TryParseD2D_RECT_F(object input, IFormatProvider provider, out D2D_RECT_F value)
    {
        value = new D2D_RECT_F();
        if (input == null)
            return false;

        if (Conversions.TryChangeType<float>(input, provider, out var f))
        {
            value = D2D_RECT_F.Thickness(f);
            return true;
        }

        if (!Conversions.TryChangeType<string>(input, provider, out var s))
            return false;

        if (s.IndexOf('#') >= 0)
        {
            var thick = s.Split(['#'], StringSplitOptions.RemoveEmptyEntries);
            if (thick.Length == 2 && Conversions.TryChangeType<float>(thick[0], provider, out var h) && Conversions.TryChangeType<float>(thick[1], provider, out var v))
            {
                value = D2D_RECT_F.Thickness(h, v);
                return true;
            }

            if (thick.Length == 4 &&
                Conversions.TryChangeType<float>(thick[0], provider, out var l) &&
                Conversions.TryChangeType<float>(thick[1], provider, out var t) &&
                Conversions.TryChangeType<float>(thick[2], provider, out var r) &&
                Conversions.TryChangeType<float>(thick[3], provider, out var b))
            {
                value = D2D_RECT_F.Thickness(l, t, r, b);
                return true;
            }
        }
        else if (s.IndexOf(';') >= 0)
        {
            var ltrb = s.Split([';'], StringSplitOptions.RemoveEmptyEntries);
            if (ltrb.Length == 4 &&
                Conversions.TryChangeType<float>(ltrb[0], provider, out var l) &&
                Conversions.TryChangeType<float>(ltrb[1], provider, out var t) &&
                Conversions.TryChangeType<float>(ltrb[2], provider, out var r) &&
                Conversions.TryChangeType<float>(ltrb[3], provider, out var b))
            {
                value.left = l;
                value.top = t;
                value.right = r;
                value.bottom = b;
                return true;
            }
        }
        else if (s.IndexOf(':') >= 0)
        {
            float getCoord(char c)
            {
                var pos = s.IndexOf(c);
                if (pos < 0)
                    return 0;

                pos++;
                if (pos < s.Length && s[pos] == ':')
                {
                    pos++;
                }

                var sb = new StringBuilder();
                while (pos < s.Length)
                {
                    if (char.IsWhiteSpace(s[pos]))
                    {
                        pos++;
                        continue;
                    }

                    if (char.IsLetter(s[pos]))
                        break;

                    sb.Append(s[pos]);
                    pos++;
                }

                if (Conversions.TryChangeType<float>(sb.ToString(), provider, out var fl))
                    return fl;

                return 0;
            }

            value.left = getCoord('L');
            value.top = getCoord('T');
            value.right = getCoord('R');
            value.bottom = getCoord('B');

            var width = getCoord('W');
            if (width != 0)
            {
                value.Width = width;
            }

            var height = getCoord('H');
            if (height != 0)
            {
                value.Height = height;
            }
            return true;
        }

        return false;
    }

    public static Size GetWinSize(this IComObject<IWICBitmapSource> bitmap) => GetWinSize(bitmap?.Object);
    public static Size GetWinSize(this IWICBitmapSource bitmap)
    {
        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap));

        bitmap.GetSize(out var width, out var height);
        return new Size(width, height);
    }

    public static POINT ToPOINT(this POINTL value) => new()
    {
        x = value.x,
        y = value.y,
    };

    public static POINTL ToPOINTL(this POINT value) => new()
    {
        x = value.x,
        y = value.y,
    };

    public static D3DCOLORVALUE ToColor(this System.Drawing.Color color) => D3DCOLORVALUE.FromArgb(color.A, color.R, color.G, color.B);
    public static Color ToColor(this D3DCOLORVALUE value) => Color.FromArgb(value.BA, value.BR, value.BG, value.BB);
    public static D3DCOLORVALUE ToColor(this Color value) => D3DCOLORVALUE.FromArgb(value.A, value.R, value.G, value.B);

    public static DWRITE_MATRIX ToMatrix(this D2D_MATRIX_3X2_F value) => new()
    {
        m11 = value._11,
        m12 = value._12,
        m21 = value._21,
        m22 = value._22,
        dx = value._31,
        dy = value._32,
    };

    public static Guid ComputeGuidHash(this string? text)
    {
        if (text == null)
            return Guid.Empty;

        using var md5 = MD5.Create();
        return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(text)));
    }

    public static Type? GetEnumeratedType(this Type collectionType)
    {
        if (collectionType == null)
            throw new ArgumentNullException(nameof(collectionType));

        if (collectionType.IsArray)
            return collectionType.GetElementType();

        var etype = GetEnumeratedItemType(collectionType);
        if (etype != null)
            return etype;

        foreach (var type in collectionType.GetInterfaces())
        {
            etype = GetEnumeratedItemType(type);
            if (etype != null)
                return etype;
        }
        return null;
    }

    private static Type? GetEnumeratedItemType(Type type)
    {
        if (!type.IsGenericType)
            return null;

        if (type.GetGenericArguments().Length != 1)
            return null;

        if (type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GetGenericArguments()[0];

        if (type.GetGenericTypeDefinition() == typeof(ICollection<>))
            return type.GetGenericArguments()[0];

        if (type.GetGenericTypeDefinition() == typeof(IList<>))
            return type.GetGenericArguments()[0];

        if (type.GetGenericTypeDefinition() == typeof(ISet<>))
            return type.GetGenericArguments()[0];

        if (type.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>))
            return type.GetGenericArguments()[0];

        if (type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            return type.GetGenericArguments()[0];

        return null;
    }
}
