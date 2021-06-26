using System;
using DirectN;

namespace Wice
{
    public abstract class Brush : IEquatable<Brush>
    {
        protected Brush()
        {
        }

        protected internal abstract IComObject<ID2D1Brush> GetBrush(RenderContext context);
        public abstract bool Equals(Brush other);
        public override abstract int GetHashCode();

        public override bool Equals(object obj) => Equals(obj as Brush);

        public static bool operator ==(Brush left, Brush right) => (left is null && right is null) || left.Equals(right);
        public static bool operator !=(Brush left, Brush right) => !(left == right);
        public static implicit operator Brush(_D3DCOLORVALUE color) => new SolidColorBrush(color);
    }
}
