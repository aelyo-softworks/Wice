namespace Wice;

public partial class RoundedRectangle : SingleShape
{
    public static VisualProperty CornerRadiusProperty { get; } = VisualProperty.Add(typeof(RoundedRectangle), nameof(CornerRadius), VisualPropertyInvalidateModes.Render, new Vector2());

    [Category(CategoryRender)]
    public Vector2 CornerRadius { get => (Vector2)GetPropertyValue(CornerRadiusProperty)!; set => SetPropertyValue(CornerRadiusProperty, value); }

    public new CompositionRoundedRectangleGeometry? Geometry => (CompositionRoundedRectangleGeometry?)base.Geometry;

    protected override CompositionGeometry? CreateGeometry() => Window?.Compositor?.CreateRoundedRectangleGeometry();

    protected override void Render()
    {
        base.Render();
        var ar = ArrangedRect;
        if (ar.IsValid && Geometry != null)
        {
            Geometry.Size = (ar.Size - Margin).ToVector2();
            Geometry.Offset = new Vector2(Margin.left, Margin.top);
            Geometry.CornerRadius = CornerRadius;
        }
    }
}
