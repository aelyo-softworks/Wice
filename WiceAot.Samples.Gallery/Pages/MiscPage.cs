using Wice.Samples.Gallery.Samples.Misc;

namespace Wice.Samples.Gallery.Pages;

public partial class MiscPage : SampleListPage
{
    public MiscPage()
    {
    }

    public override string IconText => MDL2GlyphResource.AllApps;
    public override int SortOrder => 9;

    protected override IEnumerable<SampleList> SampleLists
    {
        get
        {
            yield return new ApplicationSampleList();
            yield return new FocusSampleList();
            yield return new TitleBarSampleList();
            yield return new ToolTipSampleList();
            yield return new DragDropSampleList();
        }
    }
}
