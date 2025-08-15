namespace Wice;

/// <summary>
/// Displays a straight line shape backed by a <see cref="CompositionLineGeometry"/>.
/// </summary>
/// <remarks>
/// Lifecycle:
/// - During attachment to composition, <see cref="CreateGeometry"/> is invoked to create the line geometry.
/// - A <see cref="CompositionSpriteShape"/> is then created and added to the owning <see cref="Shape.CompositionVisual"/>
///   by the <see cref="SingleShape"/> base implementation.
/// - The geometry and sprite shape are available only while attached to composition.
/// </remarks>
/// <seealso cref="SingleShape"/>
public partial class Line : SingleShape
{
    /// <summary>
    /// Gets the line-specific composition geometry once this visual is attached to composition.
    /// </summary>
    /// <remarks>
    /// This shadows <see cref="SingleShape.Geometry"/> to expose the more specific
    /// <see cref="CompositionLineGeometry"/> type. The value is null before attachment and after detachment.
    /// </remarks>
    public new CompositionLineGeometry? Geometry => (CompositionLineGeometry?)base.Geometry;

    /// <summary>
    /// Creates a <see cref="CompositionLineGeometry"/> using the owning window's compositor.
    /// </summary>
    /// <returns>
    /// The created composition geometry, or null if the window or compositor is not available.
    /// </returns>
    /// <remarks>
    /// Per <see cref="SingleShape"/> contract, returning null will cause the attach phase to fail.
    /// </remarks>
    protected override CompositionGeometry? CreateGeometry() => Window?.Compositor?.CreateLineGeometry();
}
