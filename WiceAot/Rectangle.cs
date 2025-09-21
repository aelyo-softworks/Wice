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

    /// <summary>
    /// Creates a rectangle geometry using the owning window's compositor.
    /// </summary>
    /// <returns>
    /// A new <see cref="CompositionRectangleGeometry"/> when the window and compositor are available; otherwise, null.
    /// </returns>
    protected override CompositionGeometry? CreateGeometry() => Window?.Compositor?.CreateRectangleGeometry();

    /// <summary>
    /// Applies render-time updates to the composition state.
    /// </summary>
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
