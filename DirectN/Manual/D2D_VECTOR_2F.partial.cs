using System;
using System.Diagnostics;
using System.Numerics;

namespace DirectN
{
    [DebuggerDisplay("{ToString(),nq}")]
    public partial struct D2D_VECTOR_2F : IEquatable<D2D_VECTOR_2F>
    {
        public D2D_VECTOR_2F(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public D2D_VECTOR_2F(double x, double y)
        {
            this.x = (float)x;
            this.y = (float)y;
        }

        public D2D_VECTOR_2F(D2D_POINT_2F point0, D2D_POINT_2F point1)
        {
            x = point1.x - point0.x;
            y = point1.y - point0.y;
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

        public D2D_VECTOR_2F RotateDegree(double angle) => RotateRadian(angle * Math.PI / 180);
        public D2D_VECTOR_2F RotateRadian(double angle)
        {
            var xr = x * Math.Cos(angle) - y * Math.Sin(angle);
            var yr = x * Math.Sin(angle) + y * Math.Cos(angle);
            return new D2D_VECTOR_2F(xr, yr);
        }

        public D2D_VECTOR_2F Rotate90() => new D2D_VECTOR_2F(-y, x);
        public D2D_VECTOR_2F Rotate180() => new D2D_VECTOR_2F(-x, -y);
        public D2D_VECTOR_2F Rotate270() => new D2D_VECTOR_2F(y, -x);

        public D2D_POINT_2F TranslatePoint(D2D_POINT_2F point, float length = 1) => new D2D_POINT_2F(point.x + x * length, point.y + y * length);

        public bool Equals(D2D_VECTOR_2F other) => x.Equals(other.x) && y.Equals(other.y);
        public float[] ToArray() => new[] { x, y };
        public override bool Equals(object obj) => obj is D2D_VECTOR_2F sz && Equals(sz);
        public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode();
        public static bool operator ==(D2D_VECTOR_2F left, D2D_VECTOR_2F right) => left.Equals(right);
        public static bool operator !=(D2D_VECTOR_2F left, D2D_VECTOR_2F right) => !(left == right);
        public Vector2 ToVector2() => new Vector2(x, y);
        public Vector3 ToVector3() => new Vector3(x, y, 0);
        public D2D_SIZE_F ToD2D_SIZE_F() => new D2D_SIZE_F(x, y);
    }
}
