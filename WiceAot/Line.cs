namespace Wice;

public partial class Line : SingleShape
{
    public new CompositionLineGeometry? Geometry => (CompositionLineGeometry?)base.Geometry;

    protected override CompositionGeometry? CreateGeometry() => Window?.Compositor?.CreateLineGeometry();
}
