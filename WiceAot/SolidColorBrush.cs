namespace Wice;

/// <summary>
/// A <see cref="Brush"/> that renders using a single, immutable <see cref="D3DCOLORVALUE"/>.
/// </summary>
/// <remarks>
/// This brush materializes as a Direct2D <c>ID2D1SolidColorBrush</c> via
/// <see cref="RenderContext.CreateSolidColorBrush(D3DCOLORVALUE?)"/> when used in a rendering pass.
/// Two <see cref="SolidColorBrush"/> instances are considered equal if their <see cref="Color"/> values are equal.
/// </remarks>
/// <param name="color">The color used by this solid color brush.</param>
public class SolidColorBrush(D3DCOLORVALUE color) : Brush
{
    /// <summary>
    /// Gets the color used by this brush.
    /// </summary>
    public D3DCOLORVALUE Color { get; } = color;

    /// <summary>
    /// Creates or retrieves the underlying Direct2D brush associated with the specified render context.
    /// </summary>
    /// <param name="context">The render context providing the device context used to create the brush.</param>
    /// <returns>
    /// An <see cref="IComObject{T}"/> wrapping an <c>ID2D1Brush</c> compatible with the supplied context.
    /// </returns>
    /// <remarks>
    /// The created brush is a Direct2D solid color brush that uses <see cref="Color"/>.
    /// </remarks>
    protected internal override IComObject<ID2D1Brush> GetBrush(RenderContext context) => context.CreateSolidColorBrush(Color);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as SolidColorBrush);

    /// <summary>
    /// Determines whether the current brush is equal to another brush.
    /// </summary>
    /// <param name="other">The other brush to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="other"/> is a <see cref="SolidColorBrush"/> with the same <see cref="Color"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(Brush? other) => other is SolidColorBrush brush && Color == brush.Color;

    /// <summary>
    /// Returns a hash code for this brush.
    /// </summary>
    /// <returns>A hash code based on <see cref="Color"/>.</returns>
    public override int GetHashCode() => Color.GetHashCode();

    /// <summary>
    /// Returns a string that represents the current brush.
    /// </summary>
    /// <returns>A string representation of the underlying <see cref="Color"/>.</returns>
    public override string ToString() => Color.ToString();

    /// <summary>
    /// Implicitly converts a <see cref="D3DCOLORVALUE"/> to a <see cref="SolidColorBrush"/>.
    /// </summary>
    /// <param name="color">The color to wrap as a solid color brush.</param>
    /// <returns>A new <see cref="SolidColorBrush"/> with the specified <paramref name="color"/>.</returns>
    public static implicit operator SolidColorBrush(D3DCOLORVALUE color) => new(color);
}
