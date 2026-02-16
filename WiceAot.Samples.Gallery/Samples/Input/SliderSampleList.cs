using Wice.Samples.Gallery.Samples.Input.Slider;

namespace Wice.Samples.Gallery.Samples.Input;

public class SliderSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Equalizer;
    public override string SubTitle => "A Slider visual lets the user select from a range of values by moving a thumb visual along a track.";
    public override string Description => "A Slider can be used to select a single value from a range of values or to select a range of values using two thumbs. It can be oriented horizontally or vertically and can display tick marks and labels to indicate the values that can be selected.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new SimpleSliderSample();
            yield return new SingleSliderSample();
            yield return new VerticalSliderSample();
        }
    }
}
