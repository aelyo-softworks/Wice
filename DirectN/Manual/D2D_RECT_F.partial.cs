﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Windows.Foundation;

namespace DirectN
{
    [DebuggerDisplay("{ToString(),nq}")]
    public partial struct D2D_RECT_F : IEquatable<D2D_RECT_F>, IEquatable<Rect>
    {
        public D2D_RECT_F(float left, float top, float right, float bottom)
        {
#if DEBUG
            if (left.IsNotSet())
                throw new ArgumentException(null, nameof(left));

            if (top.IsNotSet())
                throw new ArgumentException(null, nameof(top));

            if (right.IsNotSet())
                throw new ArgumentException(null, nameof(right));

            if (bottom.IsNotSet())
                throw new ArgumentException(null, nameof(bottom));

            if (right < left)
                throw new ArgumentException(null, nameof(right));

            if (bottom < top)
                throw new ArgumentException(null, nameof(bottom));
#endif

            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public D2D_RECT_F(double left, double top, double right, double bottom)
            : this((float)left, (float)top, (float)right, (float)bottom)
        {
        }

        public D2D_RECT_F(D2D_POINT_2F location, float width, float height)
            : this(location, new D2D_SIZE_F(width, height))
        {
        }

        public D2D_RECT_F(D2D_POINT_2F leftTop, D2D_POINT_2F rightBottom)
            : this(leftTop.x, leftTop.y, rightBottom.x, rightBottom.y)
        {
        }

        public D2D_RECT_F(D2D_POINT_2F location, D2D_SIZE_F size)
        {
            left = location.x;
            top = location.y;
            right = left + size.width;
            bottom = top + size.height;
        }

        public D2D_RECT_F(D2D_SIZE_F size)
        {
            left = 0;
            top = 0;
            right = size.width;
            bottom = size.height;
        }

        public D2D_RECT_F(tagRECT rc)
        {
            left = rc.left;
            top = rc.top;
            right = left + rc.Width;
            bottom = top + rc.Height;
        }

        public D2D_RECT_F(float left, float top, D2D_SIZE_F size)
        {
#if DEBUG
            if (left.IsNotSet())
                throw new ArgumentException(null, nameof(left));

            if (top.IsNotSet())
                throw new ArgumentException(null, nameof(top));
#endif

            this.left = left;
            this.top = top;
            right = left + size.width;
            bottom = top + size.height;
        }

        public float HorizontalThickness => right + left;
        public float VerticalThickness => top + bottom;

        public float Width
        {
            get => Math.Abs(right - left);
            set
            {
                if (float.IsNaN(value) || value < 0)
                    throw new ArgumentException(null, nameof(value));

                right = left + value;
            }
        }

        public float Height
        {
            get => Math.Abs(bottom - top);
            set
            {
                if (float.IsNaN(value) || value < 0)
                    throw new ArgumentException(null, nameof(value));

                bottom = top + value;
            }
        }

        public D2D_POINT_2F CenterPoint => new D2D_POINT_2F(left + Width / 2f, top + Height / 2f);

        public D2D_SIZE_F Size { get => new D2D_SIZE_F(Width, Height); set { Width = value.width; Height = value.height; } }
        public D2D_SIZE_U SizeU => new D2D_SIZE_U(Width, Height);
        public D2D_POINT_2F LeftTop => new D2D_POINT_2F(left, top);
        public D2D_POINT_2F LeftBottom => new D2D_POINT_2F(left, bottom);
        public D2D_POINT_2F RightTop => new D2D_POINT_2F(right, top);
        public D2D_POINT_2F RightBottom => new D2D_POINT_2F(right, bottom);

        public bool IsValid => !IsInvalid;
        public bool IsInvalid => double.IsNaN(left) || double.IsNaN(top) || double.IsNaN(right) || double.IsNaN(bottom);
        public bool IsAllInvalid => double.IsNaN(left) && double.IsNaN(top) && double.IsNaN(right) && double.IsNaN(bottom);
        public bool IsSizeZero => Width.Equals(0f) && Height.Equals(0f);
        public bool IsEmpty => Width.Equals(0f) || Height.Equals(0f);
        public bool IsZero => left.Equals(0f) && top.Equals(0f) && right.Equals(0f) && bottom.Equals(0f);

        public bool IsSet => left.IsSet() && top.IsSet() && Width.IsSet() && Height.IsSet();
        public bool IsNotSet => !IsSet;

        public D2D_RECT_F TransformToBounds(ref Matrix4x4 matrix)
        {
            var xMin = left;
            var xMax = right;
            var yMin = top;
            var yMax = bottom;

            transformPoint(ref matrix, left, top);
            transformPoint(ref matrix, right, top);
            transformPoint(ref matrix, left, bottom);
            transformPoint(ref matrix, right, bottom);
            return new D2D_RECT_F(xMin, yMin, xMax, yMax);

            void transformPoint(ref Matrix4x4 m, float x, float y)
            {
                var px = x * m.M11 + y * m.M21 + m.M31;
                var py = x * m.M12 + y * m.M22 + m.M32;
                if (px > xMax)
                {
                    xMax = px;
                }
                else if (px < xMin)
                {
                    xMin = px;
                }

                if (py > yMax)
                {
                    yMax = py;
                }
                else if (py < yMin)
                {
                    yMin = py;
                }
            }
        }

        public tagRECT TotagRECT() => new tagRECT(left, top, right, bottom);
        public D2D_RECT_F ToFloorCeiling() => new D2D_RECT_F(left.Floor(), top.Floor(), right.Ceiling(), bottom.Ceiling());
        public D2D_RECT_F ToRound() => new D2D_RECT_F(left.Round(), top.Round(), right.Round(), bottom.Round());
        public D2D_RECT_F Translate(float left, float top) => Sized(this.left + left, this.top + top, Width, Height);
        public D2D_RECT_F Resize(float width, float height) => Sized(left, top, width, height);
        public D2D_RECT_F Center(D2D_RECT_F parent) => new D2D_RECT_F(parent.left + (parent.Width - Width) / 2f, parent.top + (parent.Height - Height) / 2f, Size);
        public D2D_RECT_F CenterHorizontally(D2D_RECT_F parent) => new D2D_RECT_F(parent.left + (parent.Width - Width) / 2f, top, Size);
        public D2D_RECT_F CenterVertically(D2D_RECT_F parent) => new D2D_RECT_F(left, parent.top + (parent.Height - Height) / 2f, Size);

        public void Move(D2D_VECTOR_2F vector) => Move(vector.x, vector.y);
        public void Move(float x, float y)
        {
            left += x;
            top += y;
            right += x;
            bottom += y;
        }

        public static D2D_RECT_F Sized(float left, float top, float width, float height) => new D2D_RECT_F(left, top, left + width, top + height);
        public static D2D_RECT_F Thickness(float horizontal, float vertical) => Thickness(horizontal, vertical, horizontal, vertical);
        public static D2D_RECT_F Thickness(float all) => Thickness(all, all, all, all);
        public static D2D_RECT_F Thickness(float left, float top, float right, float bottom) => new D2D_RECT_F { left = left, top = top, right = right, bottom = bottom }; // don't change how this is done (avoids validation)

        public D2D_RECT_F Deflate(float thickness) => Sized(left + thickness, top + thickness, Math.Max(0, Width - 2 * thickness), Math.Max(0, Height - 2 * thickness));
        public D2D_RECT_F Inflate(float thickness) => Deflate(-thickness);
        public D2D_RECT_F Deflate(float left, float top, float right, float bottom) => Sized(this.left + left, this.top + top, Math.Max(0, Width - 2 * right), Math.Max(0, Height - 2 * bottom));
        public D2D_RECT_F Inflate(float left, float top, float right, float bottom) => Deflate(-left, -top, -right, -bottom);
        public D2D_RECT_F Deflate(D2D_RECT_F rect) => Sized(left + rect.left, top + rect.top, Math.Max(0, Width - 2 * rect.right), Math.Max(0, Height - 2 * rect.bottom));
        public D2D_RECT_F Inflate(D2D_RECT_F rect) => Deflate(left + -rect.left, top + -rect.top, Math.Max(0, Width - 2 * -rect.right), Math.Max(0, Height - 2 * -rect.bottom));

        public D2D_RECT_F Intersect(D2D_RECT_F rect)
        {
            var l = Math.Max(left, rect.left);
            var t = Math.Max(top, rect.top);
            return new D2D_RECT_F(l, t, Math.Max(l, Math.Min(right, rect.right)), Math.Max(t, Math.Min(bottom, rect.bottom)));
        }

        public bool IntersectsWith(D2D_RECT_F rect, float minimumIntersectionSize = 0)
        {
            var i = Intersect(rect);
            return i.Width > minimumIntersectionSize && i.Height > minimumIntersectionSize;
        }

        public bool Contains(D2D_RECT_F rect) => left <= rect.left && top <= rect.top && right >= rect.right && bottom >= rect.bottom;
        public bool Contains(float left, float top) => this.left <= left && this.top <= top && right >= left && bottom >= top;
        public bool Contains(D2D_POINT_2F point) => Contains(point.x, point.y);
        public D2D_RECT_F Union(D2D_RECT_F rect)
        {
            var left = Math.Min(this.left, rect.left);
            var top = Math.Min(this.top, rect.top);
            var right = Math.Max(this.right, rect.right);
            var bottom = Math.Max(this.bottom, rect.bottom);
            return Sized(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
        }

        public D2D_RECT_F FitIn(D2D_RECT_F rect, bool shrinkOnly)
        {
            if (IsEmpty)
                return this;

            if (shrinkOnly && Width <= rect.Width && Height <= rect.Height)
                return new D2D_RECT_F(rect.left, rect.top, Width, Height);

            float w;
            float h;
            if (Width / Height >= rect.Width / rect.Height)
            {
                w = Width * rect.Width / Width;
                h = Height * rect.Width / Width;
            }
            else
            {
                w = Width * rect.Height / Height;
                h = Height * rect.Height / Height;
            }

            var l = rect.left + (rect.Width - w) / 2f;
            var t = rect.top + (rect.Height - h) / 2f;
            return new D2D_RECT_F(l, t, w + l, h + t);
        }

        public IEnumerable<D2D_POINT_2F> Points
        {
            get
            {
                yield return LeftTop;
                yield return RightTop;
                yield return RightBottom;
                yield return LeftBottom;
            }
        }

        public static D2D_RECT_F Union(IEnumerable<D2D_POINT_2F> points)
        {
            var left = float.NaN;
            var top = float.NaN;
            var right = float.NaN;
            var bottom = float.NaN;
            if (points != null)
            {
                foreach (var pt in points)
                {
                    if (pt.IsInvalid)
                        continue;

                    if (float.IsNaN(left))
                    {
                        left = pt.x;
                    }

                    if (float.IsNaN(right))
                    {
                        right = pt.x;
                    }

                    if (pt.x < left)
                    {
                        left = pt.x;
                    }
                    else if (pt.x > right)
                    {
                        right = pt.x;
                    }

                    if (float.IsNaN(top))
                    {
                        top = pt.y;
                    }

                    if (float.IsNaN(bottom))
                    {
                        bottom = pt.y;
                    }

                    if (pt.y < top)
                    {
                        top = pt.y;
                    }
                    else if (pt.y > bottom)
                    {
                        bottom = pt.y;
                    }
                }
            }

            if (float.IsNaN(left) || float.IsNaN(top) || float.IsNaN(right) || float.IsNaN(bottom))
                throw new ArgumentException(null, nameof(points));

            return new D2D_RECT_F(left, top, right, bottom);
        }

        public static D2D_RECT_F Union(IEnumerable<D2D_RECT_F> rects)
        {
            var rc = new D2D_RECT_F();
            if (rects != null)
            {
                foreach (var rect in rects)
                {
                    rc = rc.Union(rect);
                }
            }
            return rc;
        }

        public override string ToString() => "L: " + left + " T: " + top + " R: " + right + " B: " + bottom + " W: " + Width + " H: " + Height;

        public static D2D_RECT_F Parse(object input, IFormatProvider provider = null)
        {
            if (!TryParse(input, provider, out var value))
                throw new ArgumentException(null, nameof(input));

            return value;
        }

        public static bool TryParse(object input, out D2D_RECT_F value) => TryParse(input, null, out value);
        public static bool TryParse(object input, IFormatProvider provider, out D2D_RECT_F value)
        {
            value = new D2D_RECT_F();
            if (input == null)
                return false;

            if (Conversions.TryChangeType<float>(input, provider, out var f))
            {
                value = Thickness(f);
                return true;
            }

            if (!Conversions.TryChangeType<string>(input, provider, out var s))
                return false;

            if (s.IndexOf('#') >= 0)
            {
                var thick = s.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                if (thick.Length == 2 && Conversions.TryChangeType<float>(thick[0], provider, out var h) && Conversions.TryChangeType<float>(thick[1], provider, out var v))
                {
                    value = Thickness(h, v);
                    return true;
                }

                if (thick.Length == 4 &&
                    Conversions.TryChangeType<float>(thick[0], provider, out var l) &&
                    Conversions.TryChangeType<float>(thick[1], provider, out var t) &&
                    Conversions.TryChangeType<float>(thick[2], provider, out var r) &&
                    Conversions.TryChangeType<float>(thick[3], provider, out var b))
                {
                    value = Thickness(l, t, r, b);
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

        public bool Equals(Rect other) => left.Equals((float)other.Left) && top.Equals((float)other.Top) && right.Equals((float)other.Right) && bottom.Equals((float)other.Bottom);
        public bool Equals(D2D_RECT_F other) => IsInvalid && other.IsInvalid || (left.Equals(other.left) && top.Equals(other.top) && right.Equals(other.right) && bottom.Equals(other.bottom));
        public override bool Equals(object obj) => (obj is D2D_RECT_F rc && Equals(rc)) || (obj is Rect r && Equals(r));
        public override int GetHashCode() => left.GetHashCode() ^ top.GetHashCode() ^ right.GetHashCode() ^ bottom.GetHashCode();
        public static bool operator ==(D2D_RECT_F left, D2D_RECT_F right) => left.Equals(right);
        public static bool operator !=(D2D_RECT_F left, D2D_RECT_F right) => !(left == right);
        public static implicit operator D2D_RECT_F(float all) => Thickness(all);
        public static implicit operator Rect(D2D_RECT_F rc) => new Rect(rc.left, rc.top, rc.Width, rc.Height);
        public static implicit operator D2D_RECT_F(Rect rc) => new D2D_RECT_F(rc.Left, rc.Top, rc.Right, rc.Bottom);

        // margin/thickness type calculation
        public static D2D_RECT_F operator +(D2D_RECT_F left, D2D_RECT_F right)
        {
            if (left.IsInvalid || right.IsInvalid)
                return Invalid;

            return Sized(left.left - right.left, left.top - right.top, left.Width + right.HorizontalThickness, left.Height + right.VerticalThickness);
        }

        // margin/thickness type calculation
        public static D2D_RECT_F operator -(D2D_RECT_F left, D2D_RECT_F right)
        {
            if (left.IsInvalid || right.IsInvalid)
                return Invalid;

            return Sized(left.left + right.left, left.top + right.top, Math.Max(0, left.Width - right.HorizontalThickness), Math.Max(0, left.Height - right.VerticalThickness));
        }

        // margin/thickness type calculation
        public static D2D_SIZE_F operator +(D2D_SIZE_F left, D2D_RECT_F right) => new D2D_SIZE_F(left.width + right.HorizontalThickness, left.height + right.VerticalThickness);
        public static D2D_SIZE_F operator -(D2D_SIZE_F left, D2D_RECT_F right) => new D2D_SIZE_F(Math.Max(0, left.width - right.HorizontalThickness), Math.Max(0, left.height - right.VerticalThickness));

#if DEBUG
        public static readonly D2D_RECT_F Invalid = new D2D_RECT_F { left = float.NaN, top = float.NaN, right = float.NaN, bottom = float.NaN };
#else
        public static readonly D2D_RECT_F Invalid = new D2D_RECT_F(float.NaN, float.NaN, float.NaN, float.NaN);
#endif
    }
}
