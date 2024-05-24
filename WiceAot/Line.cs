namespace Wice;

public class Line : SingleShape
{
    public new CompositionLineGeometry? Geometry => (CompositionLineGeometry?)base.Geometry;

    protected override CompositionGeometry CreateGeometry() => Window?.Compositor?.CreateLineGeometry();
}
