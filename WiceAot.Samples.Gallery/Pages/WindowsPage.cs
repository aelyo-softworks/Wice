using Wice.Samples.Gallery.Samples.Windows;

namespace Wice.Samples.Gallery.Pages;

public partial class WindowsPage : SampleListPage
{
    public WindowsPage()
    {
    }

    public override string IconText => MDL2GlyphResource.SizeLegacy;
    public override int SortOrder => 4;

    protected override IEnumerable<SampleList> SampleLists
    {
        get
        {
            yield return new DialogSampleList();
            yield return new MessageBoxSampleList();
            yield return new WindowSampleList();
        }
    }
}
