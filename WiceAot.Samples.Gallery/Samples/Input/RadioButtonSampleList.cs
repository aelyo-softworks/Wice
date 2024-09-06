using Wice.Samples.Gallery.Samples.Input.RadioButton;

namespace Wice.Samples.Gallery.Samples.Input;

public class RadioButtonSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.RadioBtnOn;
    public override string SubTitle => "RadioButton visual is a specialized type of StateButton visual.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new RadioButtonsSample();
            yield return new SimpleRadioButtonSample();
        }
    }
}
