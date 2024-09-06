using Wice.Samples.Gallery.Samples.Input.CheckBox;

namespace Wice.Samples.Gallery.Samples.Input;

public class CheckBoxSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.CheckboxComposite;
    public override string SubTitle => "You can use a CheckBox visual in your application to represent options that a user can select or clear.";
    public override string Description => "You can use a CheckBox visual in your application to represent options that a user can select or clear. You can use a single check box or you can group two or more check boxes.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new ThreeStateCheckBoxSample();
            yield return new TwoStateCheckBoxSample();
        }
    }
}
