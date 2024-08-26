namespace Wice;

public partial class Rectangle : SingleShape
{
    public new CompositionRectangleGeometry? Geometry => (CompositionRectangleGeometry?)base.Geometry;

    protected override CompositionGeometry? CreateGeometry() => Window?.Compositor?.CreateRectangleGeometry();

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
