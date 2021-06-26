using System;
using System.Diagnostics;
using System.Numerics;
using Windows.Foundation;

namespace DirectN
{
    [DebuggerDisplay("{ToString(),nq}")]
    public partial struct D2D_POINT_2F : IEquatable<D2D_POINT_2F>, IEquatable<D2D_VECTOR_2F>, IEquatable<Point>, IEquatable<Vector3>
    {
        public D2D_POINT_2F(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public D2D_POINT_2F(double x, double y)
        {
            this.x = (float)x;
            this.y = (float)y;
        }

        public override string ToString() => "X: " + x + " Y: " + y;

        public bool IsValid => !IsInvalid;
        public bool IsInvalid => x.IsInvalid() || y.IsInvalid();
        public bool IsSet => x.IsSet() && y.IsSet();
        public bool IsZero => x.Equals(0f) && y.Equals(0f);
        public bool IsNotSet => x.IsNotSet() || y.IsNotSet();
        public bool Equals(D2D_VECTOR_2F other) => x.Equals(other.x) && y.Equals(other.y);
        public bool Equals(D2D_POINT_2F other) => x.Equals(other.x) && y.Equals(other.y);
        public bool Equals(Point other) => x.Equals((float)other.X) && y.Equals((float)other.Y);
        public bool Equals(Vector3 other) => x.Equals(other.X) && y.Equals(other.Y) && other.Z.Equals(0f);
        public tagPOINT TotagPOINT() => new tagPOINT((int)x, (int)y);
        public tagPOINT TotagPOINTFloor() => new tagPOINT(x.FloorI(), y.FloorI());
        public D2D_POINT_2F Floor() => new D2D_POINT_2F(x.Floor(), y.Floor());
        public D2D_POINT_2F Ceiling() => new D2D_POINT_2F(x.Ceiling(), y.Ceiling());
        public override bool Equals(object obj) => (obj is D2D_POINT_2F sz && Equals(sz)) || (obj is D2D_VECTOR_2F vc && Equals(vc)) || (obj is Point pt && Equals(pt)) || (obj is Vector3 v3 && Equals(v3));
        public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode();
        public static bool operator ==(D2D_POINT_2F left, D2D_POINT_2F right) => left.Equals(right);
        public static bool operator !=(D2D_POINT_2F left, D2D_POINT_2F right) => !left.Equals(right);
        public static D2D_POINT_2F operator +(D2D_POINT_2F left, D2D_POINT_2F right) => new D2D_POINT_2F(left.x + right.x, left.y + right.y);
        public static D2D_POINT_2F operator -(D2D_POINT_2F left, D2D_POINT_2F right) => new D2D_POINT_2F(left.x - right.x, left.y - right.y);
        public static D2D_POINT_2F operator *(D2D_POINT_2F left, D2D_MATRIX_3X2_F right) => right.Multiply(left);
        public static D2D_POINT_2F operator *(D2D_MATRIX_3X2_F left, D2D_POINT_2F right) => left.Multiply(right);
        public static implicit operator D2D_VECTOR_2F(D2D_POINT_2F pt) => new D2D_VECTOR_2F(pt.x, pt.y);
        public static implicit operator D2D_POINT_2F(D2D_VECTOR_2F vc) => new D2D_POINT_2F(vc.x, vc.y);
        public static implicit operator Point(D2D_POINT_2F pt) => new Point(pt.x, pt.y);
        public static implicit operator D2D_POINT_2F(Point pt) => new D2D_POINT_2F(pt.X, pt.Y);
        public static implicit operator Vector3(D2D_POINT_2F pt) => new Vector3(pt.x, pt.y, 0);
        public static implicit operator D2D_POINT_2F(Vector3 pt) => new D2D_POINT_2F(pt.X, pt.Y);
        public static implicit operator tagPOINT(D2D_POINT_2F pt) => new tagPOINT((int)pt.x, (int)pt.y);
        public static implicit operator D2D_POINT_2F(tagPOINT pt) => new D2D_POINT_2F(pt.x, pt.y);

        // d2d1helper.h
        public static D2D_POINT_2F TransformPoint(ref Matrix4x4 matrix, D2D_POINT_2F point) => new D2D_POINT_2F(point.x * matrix.M11 + point.y * matrix.M21 + matrix.M31, point.x * matrix.M12 + point.y * matrix.M22 + matrix.M32);
        public static D2D_POINT_2F TransformPoint(ref Matrix4x4 matrix, float x, float y) => new D2D_POINT_2F(x * matrix.M11 + y * matrix.M21 + matrix.M31, x * matrix.M12 + y * matrix.M22 + matrix.M32);
    }
}
