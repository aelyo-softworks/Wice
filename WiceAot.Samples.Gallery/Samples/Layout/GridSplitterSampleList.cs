using Wice.Samples.Gallery.Samples.Layout.GridSplitter;

namespace Wice.Samples.Gallery.Samples.Layout;

public class GridSplitterSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.HWPSplit;
    public override string SubTitle => "The GridSplitter redistributes space between child visuals of a Grid visual.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new SimpleGridSplitterSample();
        }
    }
}
