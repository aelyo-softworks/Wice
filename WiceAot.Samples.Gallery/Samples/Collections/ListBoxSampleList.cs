using Wice.Samples.Gallery.Samples.Collections.ListBox;

namespace Wice.Samples.Gallery.Samples.Collections;

public class ListBoxSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.List;
    public override string SubTitle => "A ListBox visual provides users with a list of selectable items.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new AdvancedListBoxSample();
            yield return new ListBoxSample();
        }
    }
}
