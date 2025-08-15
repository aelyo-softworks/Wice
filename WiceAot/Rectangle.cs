namespace Wice;

/// <summary>
/// A rectangular vector shape rendered via Windows Composition.
/// </summary>
/// <remarks>
/// Lifecycle:
/// - On attach to composition, a rectangle <see cref="CompositionGeometry"/> is created via <see cref="CreateGeometry"/>.
/// - A <see cref="CompositionSpriteShape"/> is then created by the base <see cref="SingleShape"/> and added to the shape visual.
/// - During <see cref="Render"/>, the geometry size is updated to match the arranged bounds.
/// </remarks>
/// <seealso cref="SingleShape"/>
public partial class Rectangle : SingleShape
{
    /// <summary>
    /// Gets the strongly-typed rectangle geometry created for this shape once attached to composition.
    /// </summary>
    /// <remarks>
    /// This property hides <see cref="SingleShape.Geometry"/> to expose a <see cref="CompositionRectangleGeometry"/>.
    /// It is null before composition attachment and after detachment.
    /// </remarks>
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
    /// <remarks>
    /// Calls the base implementation, then if the arranged rectangle is valid and the geometry exists,
    /// updates <see cref="CompositionRectangleGeometry.Size"/> to match the arranged size minus margins.
    /// </remarks>
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
