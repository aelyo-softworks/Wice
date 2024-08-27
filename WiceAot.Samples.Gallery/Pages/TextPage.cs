using Wice.Samples.Gallery.Samples.Text;

namespace Wice.Samples.Gallery.Pages;

public partial class TextPage : SampleListPage
{
    public TextPage()
    {
    }

    public override string IconText => MDL2GlyphResource.Font;
    public override int SortOrder => 5;

    protected override IEnumerable<SampleList> SampleLists
    {
        get
        {
            yield return new RichTextBoxSampleList();
            yield return new TextBoxSampleList();
        }
    }
}
