using Wice.Samples.Gallery.Samples.Input.Button;

namespace Wice.Samples.Gallery.Samples.Input;

public class ButtonSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.ButtonB;
    public override string SubTitle => "A Button visual reacts to user input from a mouse, keyboard, or other input device and raises a Click event.";
    public override string Description => "A Button visual reacts to user input from a mouse, keyboard, or other input device and raises a Click event. A ButtonBase is a basic visual that can contain any content, from text to complex content, such as images and other visuals.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new ButtonDockSample();
            yield return new SimpleButtonSample();
        }
    }
}
