namespace Wice;

/// <summary>
/// A rectangular vector shape rendered via Windows Composition.
/// </summary>
public partial class Rectangle : SingleShape
{
    /// <summary>
    /// Gets the strongly-typed rectangle geometry created for this shape once attached to composition.
    /// </summary>
    public new CompositionRectangleGeometry? Geometry => (CompositionRectangleGeometry?)base.Geometry;

    /// <inheritdoc/>
    protected override CompositionGeometry? CreateGeometry() => Window?.Compositor?.CreateRectangleGeometry();

    /// <inheritdoc/>
    protected override void Render()
    {
        base.Render();
        var ar = ArrangedRect;
        if (ar.IsValid && Geometry != null)
        {
            Geometry.Size = (ar.Size - Margin).ToVector2();
        }
    }
}
