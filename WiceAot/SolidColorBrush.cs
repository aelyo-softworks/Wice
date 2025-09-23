namespace Wice;

/// <summary>
/// A <see cref="Brush"/> that renders using a single, immutable <see cref="D3DCOLORVALUE"/>.
/// </summary>
/// <param name="color">The color used by this solid color brush.</param>
public class SolidColorBrush(D3DCOLORVALUE color) : Brush
{
    /// <summary>
    /// Gets the color used by this brush.
    /// </summary>
    public D3DCOLORVALUE Color { get; } = color;

    /// <inheritdoc/>
    protected internal override IComObject<ID2D1Brush> GetBrush(RenderContext context) => context.CreateSolidColorBrush(Color);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as SolidColorBrush);

    /// <inheritdoc/>
    public override bool Equals(Brush? other) => other is SolidColorBrush brush && Color == brush.Color;

    /// <inheritdoc/>
    public override int GetHashCode() => Color.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => Color.ToString();

    /// <summary>
    /// Implicitly converts a <see cref="D3DCOLORVALUE"/> to a <see cref="SolidColorBrush"/>.
    /// </summary>
    /// <param name="color">The color to wrap as a solid color brush.</param>
    /// <returns>A new <see cref="SolidColorBrush"/> with the specified <paramref name="color"/>.</returns>
    public static implicit operator SolidColorBrush(D3DCOLORVALUE color) => new(color);
}
