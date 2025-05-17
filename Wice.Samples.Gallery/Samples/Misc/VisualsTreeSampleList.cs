using Wice.Samples.Gallery.Samples.Misc.VisualsTree;

namespace Wice.Samples.Gallery.Samples.Misc
{
    public class VisualsTreeSampleList : SampleList
    {
        public override string IconText => MDL2GlyphResource.Bug;
        public override string SubTitle => "The Visuals Tree is an integrated diagnostic tool that allows you browse the visual tree of the current WICE application, and modify visuals.";

        protected override IEnumerable<Sample> Types
        {
            get
            {
                yield return new VisualsTreeSample();
            }
        }
    }
}
