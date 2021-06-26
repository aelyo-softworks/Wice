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

        public override string ToString() => "X: " + x + " Y: " + y;

        public bool IsValid => !IsInvalid;
        public bool IsInvalid => x.IsInvalid() || y.IsInvalid();
        public bool IsSet => x.IsSet() && y.IsSet();
        public bool IsNotSet => x.IsNotSet() || y.IsNotSet();
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
