using DirectN;

namespace Wice.Samples.Gallery.Pages
{
    public class LayoutPage : SampleListPage
    {
        public LayoutPage()
        {
        }

        public override string IconText => MDL2GlyphResource.PreviewLink;
        public override int SortOrder => 2;
    }
}
