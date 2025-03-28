using Wice.Samples.Gallery.Samples.Collections.EnumListBox;

namespace Wice.Samples.Gallery.Samples.Collections;

public class EnumListBoxSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.List;
    public override string SubTitle => "The EnumListBox visual is a ListBox visual that uses a .NET enum type to determine the child items.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new EnumListBoxSample();
        }
    }
}
