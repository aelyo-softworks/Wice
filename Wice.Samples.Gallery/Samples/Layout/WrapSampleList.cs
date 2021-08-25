using DirectN;

namespace Wice.Samples.Gallery.Samples.Layout
{
    public class WrapSampleList : SampleList
    {
        public override string IconText => MDL2GlyphResource.CollatePortraitSeparated;
        public override string SubTitle => "The Wrap visual positions child visuals in sequential position, breaking content at the edge of its containing box.";
        public override string Description => "The Wrap visual positions child visual in sequential position from left to right, or top to bottom depending on orientation, breaking content when necessary at the edge of its containing box.";
    }
}
