namespace Wice;

/// <summary>
/// A vector <see cref="Shape"/> that renders an ellipse centered within its arranged bounds.
/// </summary>
public partial class Ellipse : SingleShape
{
    /// <summary>
    /// Dynamic property descriptor for <see cref="Radius"/>.
    /// </summary>
    public static VisualProperty RadiusProperty { get; } = VisualProperty.Add(typeof(Ellipse), nameof(Radius), VisualPropertyInvalidateModes.Render, new Vector2(), ValidateEmptyVector2);

    /// <summary>
    /// Dynamic property descriptor for <see cref="RadiusOffset"/>.
    /// </summary>
    public static VisualProperty RadiusOffsetProperty { get; } = VisualProperty.Add(typeof(Ellipse), nameof(RadiusOffset), VisualPropertyInvalidateModes.Render, new Vector2(), ValidateEmptyVector2);

    /// <summary>
    /// Gets the underlying composition ellipse geometry for this shape, when available.
    /// </summary>
    public new CompositionEllipseGeometry? Geometry => (CompositionEllipseGeometry?)base.Geometry;

    /// <summary>
    /// Gets or sets the ellipse radius along the X and Y axes, in DIPs.
    /// </summary>
    [Category(CategoryLayout)]
    public Vector2 Radius { get => (Vector2)GetPropertyValue(RadiusProperty)!; set => SetPropertyValue(RadiusProperty, value); }

    /// <summary>
    /// Gets or sets the additive offset applied to the computed <see cref="Radius"/>.
    /// </summary>
    public Vector2 RadiusOffset { get => (Vector2)GetPropertyValue(RadiusOffsetProperty)!; set => SetPropertyValue(RadiusOffsetProperty, value); }

    /// <inheritdoc/>
    protected override CompositionGeometry? CreateGeometry() => Window?.Compositor?.CreateEllipseGeometry();

    /// <inheritdoc/>
    protected override void Render()
    {
        base.Render();
        var ar = ArrangedRect;
        var radius = Radius;
        if (ar.IsValid && Geometry != null)
        {
            var size = ar.Size - Margin;
            Geometry.Center = new Vector2(size.width / 2, size.height / 2);
            if (radius.X == 0 && radius.Y == 0)
            {
                // make sure the circle is inside the container, so remove stroke's semi width
                Geometry.Radius = new Vector2(Math.Max(0, Geometry.Center.X - StrokeThickness / 2 + RadiusOffset.X), Math.Max(0, Geometry.Center.Y - StrokeThickness / 2 + RadiusOffset.Y));
            }
            else
            {
                Geometry.Radius = new Vector2(Math.Max(0, radius.X + RadiusOffset.X), Math.Max(0, radius.Y + RadiusOffset.Y));
            }
        }
    }
}
