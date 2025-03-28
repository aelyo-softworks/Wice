using Wice.Samples.Gallery.Samples.Input.ToggleSwitch;

namespace Wice.Samples.Gallery.Samples.Input;

public class ToggleSwitchSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Switch;
    public override string SubTitle => "The ToggleSwitch visual represents a switch that can be toggled between two states.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new GroupedToggleSwitchesSample();
            yield return new ToggleSwitchSample();
        }
    }
}
