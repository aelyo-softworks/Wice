using DirectN;

namespace Wice.Samples.Gallery.Pages
{
    public class ScrollingPage : SampleListPage
    {
        public ScrollingPage()
        {
        }

        public override string IconText => MDL2GlyphResource.ScrollUpDown;
        public override int SortOrder => 8;
    }
}
