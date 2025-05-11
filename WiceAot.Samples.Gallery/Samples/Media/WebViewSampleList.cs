using Wice.Samples.Gallery.Samples.Media.WebView;

namespace Wice.Samples.Gallery.Samples.Media;

public class WebViewSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.World;
    public override string Description => "You can use a WebView visual to display web content (HTML, CSS, and JavasScript) content.";
    public override string SubTitle => "A visual that supports displaying web content (HTML, CSS, and JavasScript) content.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new WebViewSample();
        }
    }
}
