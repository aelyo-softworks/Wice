using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DirectN
{
    // = also RECTL
    [StructLayout(LayoutKind.Sequential)]
    public struct tagRECT : IEquatable<tagRECT>
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public int Width
        {
            get => Math.Abs(right - left);
            set
            {
                if (value < 0)
                    throw new ArgumentException(null, nameof(value));

                right = left + value;
            }
        }

        public int Height
        {
            get => Math.Abs(bottom - top);
            set
            {
                if (value < 0)
                    throw new ArgumentException(null, nameof(value));

                bottom = top + value;
            }
        }

        public tagRECT(int left, int top, int right, int bottom)
        {
#if DEBUG
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

        public tagRECT(int left, int top, tagSIZE size)
        {
            this.left = left;
            this.top = top;
            right = (int)(left + size.width);
            bottom = (int)(top + size.height);
        }

        public tagRECT(tagPOINT pt, tagSIZE size)
        {
            left = pt.x;
            top = pt.y;
            right = (int)(left + size.width);
            bottom = (int)(top + size.height);
        }

        public tagRECT(float left, float top, float right, float bottom)
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

            this.left = left.FloorI();
            this.top = top.FloorI();
            this.right = right.CeilingI();
            this.bottom = bottom.CeilingI();
        }

        public bool IsEmpty => Width == 0 || Height == 0;
        public bool Equals(tagRECT other) => left == other.left && top == other.top && right == other.right && bottom == other.bottom;
        public override bool Equals(object obj) => obj is tagRECT rc && Equals(rc);
        public override int GetHashCode() => left.GetHashCode() ^ top.GetHashCode() ^ right.GetHashCode() ^ bottom.GetHashCode();
        public override string ToString() => "L:" + left + " T:" + top + " W:" + Width + " H:" + Height + " R:" + right + " B:" + bottom;

        public static bool operator ==(tagRECT left, tagRECT right) => left.Equals(right);
        public static bool operator !=(tagRECT left, tagRECT right) => !left.Equals(right);

        public tagPOINT Position => new tagPOINT(left, top);
        public tagSIZE Size => new tagSIZE(Width, Height);
        public tagPOINT LeftTop => new tagPOINT(left, top);
        public tagPOINT LeftBottom => new tagPOINT(left, bottom);
        public tagPOINT RightTop => new tagPOINT(right, top);
        public tagPOINT RightBottom => new tagPOINT(right, bottom);
        public tagRECT Abs => new tagRECT(Math.Abs(left), Math.Abs(top), Math.Abs(right), Math.Abs(bottom));

        public D2D_RECT_F ToD2D_RECT_F() => new D2D_RECT_F(left, top, right, bottom);
        public Vector2 ToVector2() => new Vector2(Width, Height);

        public D2D_RECT_F PixelToHiMetric()
        {
            var dpi = D2D1Functions.Dpi;
            return new D2D_RECT_F(
                tagSIZE.HIMETRIC_PER_INCH * left / dpi.width,
                tagSIZE.HIMETRIC_PER_INCH * top / dpi.height,
                tagSIZE.HIMETRIC_PER_INCH * right / dpi.width,
                tagSIZE.HIMETRIC_PER_INCH * bottom / dpi.height
                );
        }

        public D2D_RECT_F HiMetricToPixel()
        {
            var dpi = D2D1Functions.Dpi;
            return new D2D_RECT_F(
                left * dpi.width / tagSIZE.HIMETRIC_PER_INCH,
                top * dpi.height / tagSIZE.HIMETRIC_PER_INCH,
                right * dpi.width / tagSIZE.HIMETRIC_PER_INCH,
                bottom * dpi.height / tagSIZE.HIMETRIC_PER_INCH);
        }

        public D2D_RECT_F PixelToDip()
        {
            var scale = D2D1Functions.DpiScale;
            if (scale.width == 1 && scale.height == 1)
                return ToD2D_RECT_F();

            return new D2D_RECT_F(left / scale.width, top / scale.height, right / scale.width, bottom / scale.height);
        }

        public D2D_RECT_F DipToPixel()
        {
            var scale = D2D1Functions.DpiScale;
            if (scale.width == 1 && scale.height == 1)
                return ToD2D_RECT_F();

            return new D2D_RECT_F(left * scale.width, top * scale.height, right * scale.width, bottom * scale.height);
        }
    }
}
