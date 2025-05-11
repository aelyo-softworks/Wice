using Wice.Samples.Gallery.Samples.Media;

namespace Wice.Samples.Gallery.Pages;

public partial class MediaPage : SampleListPage
{
    public MediaPage()
    {
    }

    public override string IconText => MDL2GlyphResource.Slideshow;
    public override int SortOrder => 6;

    protected override IEnumerable<SampleList> SampleLists
    {
        get
        {
            yield return new BrushSampleList();
            yield return new ImageSampleList();
            yield return new ShapeSampleList();
            yield return new SvgImageSampleList();
            yield return new WebViewSampleList();
            yield return new PdfViewSampleList();
        }
    }
}
