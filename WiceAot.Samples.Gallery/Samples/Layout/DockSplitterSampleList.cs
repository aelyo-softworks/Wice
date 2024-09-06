using Wice.Samples.Gallery.Samples.Layout.DockSplitter;

namespace Wice.Samples.Gallery.Samples.Layout;

public class DockSplitterSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.HWPSplit;
    public override string SubTitle => "The DockSplitter redistributes space between child visuals of a Dock visual.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new SimpleDockSplitterSample();
        }
    }
}
