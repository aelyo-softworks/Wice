using Wice.Samples.Gallery.Samples.Layout.ScrollViewer;

namespace Wice.Samples.Gallery.Samples.Layout;

public class ScrollViewerSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.ScrollUpDown;
    public override string SubTitle => "The ScrollViewer visual creates a scrollable region wherein content can be scrolled horizontally or vertically.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new ScrollViewerBasicSample();
        }
    }
}
