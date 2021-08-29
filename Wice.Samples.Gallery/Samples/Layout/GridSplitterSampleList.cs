using DirectN;

namespace Wice.Samples.Gallery.Samples.Layout
{
    public class GridSplitterSampleList : SampleList
    {
        public override bool IsEnabled => false;
        public override string IconText => MDL2GlyphResource.HWPSplit;
        public override string SubTitle => "The GridSplitter redistributes space between child visuals of a Grid visual.";
    }
}
