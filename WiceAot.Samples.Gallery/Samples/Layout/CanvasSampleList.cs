using Wice.Samples.Gallery.Samples.Layout.Canvas;

namespace Wice.Samples.Gallery.Samples.Layout;

public class CanvasSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.PageMarginPortraitNormal;
    public override string Description => "The Canvas provides absolute positioning of visuals or content. Content is positioned relative to the Canvas using the Canvas.Top, Left Right and Bottom properties.";
    public override string SubTitle => "A layout panel that supports absolute positioning of child elements relative to the top left corner of the canvas.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new SimpleCanvasSample();
            yield return new BoundsCanvasSample();
        }
    }
}
