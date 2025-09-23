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

    /// <inheritdoc/>
    protected override CompositionGeometry? CreateGeometry() => Window?.Compositor?.CreateLineGeometry();
}
