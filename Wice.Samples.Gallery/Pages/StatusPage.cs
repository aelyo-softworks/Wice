using DirectN;

namespace Wice.Samples.Gallery.Pages
{
    public class StatusPage : SampleListPage
    {
        public StatusPage()
        {
        }

        public override string IconText => MDL2GlyphResource.Message;
        public override int SortOrder => 3;
    }
}
