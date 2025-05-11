using Wice.Samples.Gallery.Samples.Media.PdfView;

namespace Wice.Samples.Gallery.Samples.Media;

public class PdfViewSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.PDF;
    public override string Description => "You can use a PdfView visual to display a PDF content from a file or a stream.";
    public override string SubTitle => "A visual that supports displaying a PDF pages. It's a read-only high performance view.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new PdfViewSample();
        }
    }
}
