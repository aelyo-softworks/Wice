namespace Wice;

/// <summary>
/// Displays a straight line shape backed by a <see cref="CompositionLineGeometry"/>.
/// </summary>
public partial class Line : SingleShape
{
    /// <summary>
    /// Gets the line-specific composition geometry once this visual is attached to composition.
    /// </summary>
    public new CompositionLineGeometry? Geometry => (CompositionLineGeometry?)base.Geometry;

    /// <summary>
    /// Creates a <see cref="CompositionLineGeometry"/> using the owning window's compositor.
    /// </summary>
    /// <returns>
    /// The created composition geometry, or null if the window or compositor is not available.
    /// </returns>
    protected override CompositionGeometry? CreateGeometry() => Window?.Compositor?.CreateLineGeometry();
}
