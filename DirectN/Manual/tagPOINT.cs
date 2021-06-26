using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
#pragma warning disable IDE1006 // Naming Styles
    public struct tagPOINT : IEquatable<tagPOINT>
#pragma warning restore IDE1006 // Naming Styles
    {
        public int x;
        public int y;

        public tagPOINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool IsZero => x == 0 && y == 0;
        public override string ToString() => "X=" + x + ",Y=" + y;
        public bool Equals(tagPOINT other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is tagPOINT sz && Equals(sz);
        public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode();
        public D2D_POINT_2F ToD2D_POINT_2F() => new D2D_POINT_2F(x, y);

        public static bool operator ==(tagPOINT left, tagPOINT right) => left.Equals(right);
        public static bool operator !=(tagPOINT left, tagPOINT right) => !left.Equals(right);
        public static implicit operator tagPOINT(Point pt) => new tagPOINT(pt.X, pt.Y);
        public static implicit operator Point(tagPOINT pt) => new Point(pt.x, pt.y);
    }
}
