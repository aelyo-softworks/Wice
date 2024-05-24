namespace Wice;

public class Ellipse : SingleShape
{
    public static VisualProperty RadiusProperty { get; } = VisualProperty.Add(typeof(Ellipse), nameof(Radius), VisualPropertyInvalidateModes.Render, new Vector2());
    public static VisualProperty RadiusOffsetProperty { get; } = VisualProperty.Add(typeof(Ellipse), nameof(RadiusOffset), VisualPropertyInvalidateModes.Render, new Vector2());

    public new CompositionEllipseGeometry? Geometry => (CompositionEllipseGeometry)base.Geometry;

    [Category(CategoryLayout)]
    public Vector2 Radius { get => (Vector2)GetPropertyValue(RadiusProperty)!; set => SetPropertyValue(RadiusProperty, value); }

    // useful to define radius with respect to parent/max size
    public Vector2 RadiusOffset { get => (Vector2)GetPropertyValue(RadiusOffsetProperty)!; set => SetPropertyValue(RadiusOffsetProperty, value); }

    protected override CompositionGeometry? CreateGeometry() => Window?.Compositor?.CreateEllipseGeometry();

    protected override void Render()
    {
        base.Render();
        var ar = ArrangedRect;
        if (ar.IsValid)
        {
            var size = ar.Size - Margin;
            Geometry.Center = new Vector2(size.width / 2, size.height / 2);
            if (Radius.X == 0 && Radius.Y == 0)
            {
                // make sure the circle is inside the container, so remove stroke's semi width
                Geometry.Radius = new Vector2(Math.Max(0, Geometry.Center.X - StrokeThickness / 2 + RadiusOffset.X), Math.Max(0, Geometry.Center.Y - StrokeThickness / 2 + RadiusOffset.Y));
            }
            else
            {
                Geometry.Radius = new Vector2(Math.Max(0, Radius.X + RadiusOffset.X), Math.Max(0, Radius.Y + RadiusOffset.Y));
            }
        }
    }
}
