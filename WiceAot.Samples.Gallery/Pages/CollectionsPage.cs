using Wice.Samples.Gallery.Samples.Collections;

namespace Wice.Samples.Gallery.Pages;

public partial class CollectionsPage : SampleListPage
{
    public CollectionsPage()
    {
    }

    public override string IconText => MDL2GlyphResource.GridView;
    public override int SortOrder => 7;

    protected override IEnumerable<SampleList> SampleLists
    {
        get
        {
            yield return new CheckBoxListSampleList();
            yield return new EnumListBoxSampleList();
            yield return new FlagsEnumListBoxSampleList();
            yield return new ListBoxSampleList();
        }
    }
}
