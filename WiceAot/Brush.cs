namespace Wice;

/// <summary>
/// Represents the abstract base type for brushes used by the rendering system,
/// providing a way to materialize Direct2D brush instances for a given <see cref="RenderContext"/>.
/// </summary>
/// <remarks>
/// Subclasses implement brush-specific behavior and equality semantics. The underlying native brush
/// object is obtained through <see cref="GetBrush(RenderContext)"/>.
/// </remarks>
public abstract class Brush : IEquatable<Brush>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Brush"/> class.
    /// </summary>
    protected Brush()
    {
    }

    /// <summary>
    /// Creates or retrieves the underlying Direct2D brush associated with the specified render context.
    /// </summary>
    /// <param name="context">The render context that provides the device context used to create the brush.</param>
    /// <returns>
    /// An <see cref="IComObject{T}"/> wrapping an <c>ID2D1Brush</c> that can be used with the supplied context.
    /// </returns>
    protected internal abstract IComObject<ID2D1Brush> GetBrush(RenderContext context);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as Brush);

    /// <summary>
    /// Determines whether the current brush is equal to another brush.
    /// </summary>
    /// <param name="other">The other brush to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if the brushes are considered equal; otherwise, <see langword="false"/>.
    /// </returns>
    public abstract bool Equals(Brush? other);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    /// <remarks>
    /// Implementations must ensure that equal brushes (as determined by <see cref="Equals(Brush?)"/>)
    /// return the same hash code.
    /// </remarks>
    public override abstract int GetHashCode();

    /// <summary>
    /// Determines whether two <see cref="Brush"/> instances are equal.
    /// </summary>
    /// <param name="left">The first brush to compare.</param>
    /// <param name="right">The second brush to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both are <see langword="null"/> or if <paramref name="left"/> equals <paramref name="right"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Brush? left, Brush? right) => (left is null && right is null) || left?.Equals(right) == true;

    /// <summary>
    /// Determines whether two <see cref="Brush"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first brush to compare.</param>
    /// <param name="right">The second brush to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the brushes are not equal; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Brush? left, Brush? right) => !(left == right);

    /// <summary>
    /// Implicitly converts a <c>D3DCOLORVALUE</c> into a <see cref="SolidColorBrush"/>.
    /// </summary>
    /// <param name="color">The color value to convert.</param>
    /// <returns>
    /// A <see cref="Brush"/> instance representing a solid color brush with the specified color.
    /// </returns>
    public static implicit operator Brush(D3DCOLORVALUE color) => new SolidColorBrush(color);
}
