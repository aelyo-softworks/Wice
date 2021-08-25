using DirectN;

namespace Wice.Samples.Gallery.Pages
{
    public class MediaPage : SampleListPage
    {
        public MediaPage()
        {
        }

        public override string IconText => MDL2GlyphResource.Slideshow;
        public override int SortOrder => 6;
    }
}
