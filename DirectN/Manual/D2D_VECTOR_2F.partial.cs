using System;
using System.Diagnostics;
using System.Numerics;

namespace DirectN
{
    [DebuggerDisplay("{ToString(),nq}")]
    public partial struct D2D_VECTOR_2F : IEquatable<D2D_VECTOR_2F>
    {
        public D2D_VECTOR_2F(float x, float y, bool normalize = false)
        {
            this.x = x;
            this.y = y;

            if (normalize)
            {
                var len = Length;
                if (len > 0)
                {
                    this.x /= len;
                    this.y /= len;
                }
            }
        }

        public D2D_VECTOR_2F(double x, double y, bool normalize = false)
        {
            this.x = (float)x;
            this.y = (float)y;

            if (normalize)
            {
                var len = Length;
                if (len > 0)
                {
                    this.x /= len;
                    this.y /= len;
                }
            }
        }

        public D2D_VECTOR_2F(D2D_POINT_2F point0, D2D_POINT_2F point1, bool normalize = false)
        {
            x = point1.x - point0.x;
            y = point1.y - point0.y;

            if (normalize)
            {
                var len = Length;
                if (len > 0)
                {
                    x /= len;
                    y /= len;
                }
            }
        }

        public override string ToString() => "X: " + x + " Y: " + y;

        public bool IsValid => !IsInvalid;
        public bool IsInvalid => x.IsInvalid() || y.IsInvalid();
        public bool IsSet => x.IsSet() && y.IsSet();
        public bool IsNotSet => x.IsNotSet() || y.IsNotSet();
        public float Length => (float)Math.Sqrt(x * x + y * y);

        public D2D_VECTOR_2F Normalize()
        {
            var len = Length;
            if (len == 0)
                return new D2D_VECTOR_2F();

            return new D2D_VECTOR_2F(x / len, y / len);
        }

        public D2D_VECTOR_2F RotateDegree(double degreeAngle) => RotateRadian(degreeAngle * Math.PI / 180);
        public D2D_VECTOR_2F RotateRadian(double radianAngle)
        {
            var xr = x * Math.Cos(radianAngle) - y * Math.Sin(radianAngle);
            var yr = x * Math.Sin(radianAngle) + y * Math.Cos(radianAngle);
            return new D2D_VECTOR_2F(xr, yr);
        }

        public D2D_VECTOR_2F Rotate90() => new D2D_VECTOR_2F(-y, x);
        public D2D_VECTOR_2F Rotate180() => new D2D_VECTOR_2F(-x, -y);
        public D2D_VECTOR_2F Rotate270() => new D2D_VECTOR_2F(y, -x);

        public D2D_VECTOR_2F Add(D2D_VECTOR_2F other) => new D2D_VECTOR_2F(x + other.x, y + other.y);
        public D2D_VECTOR_2F Remove(D2D_VECTOR_2F other) => new D2D_VECTOR_2F(x - other.x, y - other.y);
        public float Dot(D2D_VECTOR_2F other) => x * other.x + y * other.y;
        public float Cross(D2D_VECTOR_2F other) => x * other.y - y * other.x;
        public float AngleRadian(D2D_VECTOR_2F other) => (float)Math.Atan2(Cross(other), Dot(other));
        public float AngleDegree(D2D_VECTOR_2F other) => (float)(Math.Atan2(Cross(other), Dot(other)) * 180 / Math.PI);

        public D2D_POINT_2F TranslatePoint(D2D_POINT_2F point, float length = 1) => new D2D_POINT_2F(point.x + x * length, point.y + y * length);

        public bool Equals(D2D_VECTOR_2F other) => x.Equals(other.x) && y.Equals(other.y);
        public float[] ToArray() => new[] { x, y };
        public Vector2 ToVector2() => new Vector2(x, y);
        public Vector3 ToVector3() => new Vector3(x, y, 0);
        public D2D_SIZE_F ToD2D_SIZE_F() => new D2D_SIZE_F(x, y);
        public override bool Equals(object obj) => obj is D2D_VECTOR_2F sz && Equals(sz);
        public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode();
        public static bool operator ==(D2D_VECTOR_2F left, D2D_VECTOR_2F right) => left.Equals(right);
        public static bool operator !=(D2D_VECTOR_2F left, D2D_VECTOR_2F right) => !(left == right);
        public static D2D_VECTOR_2F operator +(D2D_VECTOR_2F left, D2D_VECTOR_2F right) => left.Add(right);
        public static D2D_VECTOR_2F operator -(D2D_VECTOR_2F left, D2D_VECTOR_2F right) => left.Remove(right);
        public static D2D_POINT_2F operator +(D2D_VECTOR_2F left, D2D_POINT_2F point) => left.TranslatePoint(point);
    }
}
