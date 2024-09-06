using Wice.Samples.Gallery.Samples.Media.Shape;

namespace Wice.Samples.Gallery.Samples.Media;

public class ShapeSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.OEM;
    public override string Description => "Wice provides a number of ready-to-use Shape objects. All inherit from the Shape class. Available shape objects include Ellipse, Line, Path, Rectangle and RoundedRectangle.";
    public override string SubTitle => "A Shape visual supports displaying a geometry (rectangle, ellipse, etc.) with stroke, thickness and fill brush properties.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new EllipseRectangleShapeSample();
            yield return new LineShapeSample();
            yield return new RectangleShapeSample();
            yield return new RoundedRectangleShapeSample();
        }
    }
}
