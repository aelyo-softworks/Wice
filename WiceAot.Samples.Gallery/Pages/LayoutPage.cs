using Wice.Samples.Gallery.Samples.Layout;

namespace Wice.Samples.Gallery.Pages;

public partial class LayoutPage : SampleListPage
{
    public LayoutPage()
    {
    }

    public override string IconText => MDL2GlyphResource.PreviewLink;
    public override int SortOrder => 2;

    protected override IEnumerable<SampleList> SampleLists
    {
        get
        {
            yield return new BorderSampleList();
            yield return new CanvasSampleList();
            yield return new DockSampleList();
            yield return new DockSplitterSampleList();
            yield return new GridSampleList();
            yield return new GridSplitterSampleList();
            yield return new ScrollViewerSampleList();
            yield return new StackSampleList();
            yield return new UniformGridSampleList();
            yield return new WrapSampleList();
        }
    }
}
