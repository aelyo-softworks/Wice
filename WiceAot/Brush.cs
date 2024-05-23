namespace Wice;

public abstract class Brush : IEquatable<Brush>
{
    protected Brush()
    {
    }

    protected internal abstract IComObject<ID2D1Brush> GetBrush(RenderContext context);
    public override bool Equals(object? obj) => Equals(obj as Brush);
    public abstract bool Equals(Brush? other);
    public override abstract int GetHashCode();

    public static bool operator ==(Brush left, Brush right) => (left is null && right is null) || left?.Equals(right) == true;
    public static bool operator !=(Brush left, Brush right) => !(left == right);
    public static implicit operator Brush(D3DCOLORVALUE color) => new SolidColorBrush(color);
}
