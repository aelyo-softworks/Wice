using DirectN;

namespace Wice.Samples.Gallery.Samples.Media
{
    public class BrushSampleList : SampleList
    {
        public override string IconText => MDL2GlyphResource.BrushSize;
        public override string Description => "A Brush paints an area with its output. Different brushes have different types of output. Some brushes paint an area with a solid color, others with a gradient, pattern, image, or drawing.";
        public override string SubTitle => "Brushes enable you to paint user visuals with anything from simple, solid colors to complex sets of patterns and images.";
    }
}
