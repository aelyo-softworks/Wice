using Wice.Samples.Gallery.Samples.Collections.CheckBoxList;

namespace Wice.Samples.Gallery.Samples.Collections;

public class CheckBoxListSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.CheckList;
    public override string SubTitle => "The CheckListBox visual is a ListBox visual that uses CheckBox visuals for items.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new CheckBoxListSample();
        }
    }
}
