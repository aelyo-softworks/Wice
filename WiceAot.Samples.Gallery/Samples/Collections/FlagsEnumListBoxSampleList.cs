using Wice.Samples.Gallery.Samples.Collections.FlagsEnumListBox;

namespace Wice.Samples.Gallery.Samples.Collections;

public class FlagsEnumListBoxSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.CheckList;
    public override string SubTitle => "The FlagsEnumListBox visual is a CheckBoxList visual that uses a multi-valued .NET enum type (with [Flags] attribute) to detemine the child items.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new FlagsEnumListBoxSample();
        }
    }
}
