using System;
using System.Numerics;
using System.Text;
using DirectN;
using Windows.Foundation;
using Windows.UI;

namespace Wice.Utilities
{
    public static class Extensions
    {
        public static Size ToSize(this D2D_SIZE_F size) => new Size(size.width, size.height);
        public static Size ToSize(this tagSIZE size) => new Size(size.width, size.height);
        public static D2D_RECT_F ToRound(this D2D_RECT_F rect) => new D2D_RECT_F(rect.left.Round(), rect.top.Round(), rect.right.Round(), rect.bottom.Round());
        public static System.Drawing.Point ToPoint(this tagPOINT pt) => new System.Drawing.Point(pt.x, pt.y);
        public static tagPOINT TotagPOINT(this System.Drawing.Point pt) => new tagPOINT(pt.X, pt.Y);
        public static Vector2 ToVector2(this D2D_SIZE_F size) => new Vector2(size.width, size.height);
        public static Vector3 ToVector3(this D2D_SIZE_F size) => new Vector3(size.width, size.height, 0);

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
                var thick = s.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
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
                var ltrb = s.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
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

        public static _D3DCOLORVALUE ToColor(this System.Drawing.Color color) => _D3DCOLORVALUE.FromArgb(color.A, color.R, color.G, color.B);
        public static Color ToColor(this _D3DCOLORVALUE value) => Color.FromArgb(value.BA, value.BR, value.BG, value.BB);
        public static _D3DCOLORVALUE ToColor(this Color value) => _D3DCOLORVALUE.FromArgb(value.A, value.R, value.G, value.B);
    }
}
