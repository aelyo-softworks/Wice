namespace Wice;

public class SolidColorBrush(D3DCOLORVALUE color) : Brush
{
    public D3DCOLORVALUE Color { get; } = color;

    protected internal override IComObject<ID2D1Brush> GetBrush(RenderContext context) => context.CreateSolidColorBrush(Color);

    public override bool Equals(object? obj) => Equals(obj as SolidColorBrush);
    public override bool Equals(Brush? other) => other is SolidColorBrush brush && Color == brush.Color;
    public override int GetHashCode() => Color.GetHashCode();
    public override string ToString() => Color.ToString();

    public static implicit operator SolidColorBrush(D3DCOLORVALUE color) => new(color);
}
