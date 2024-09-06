using Wice.Samples.Gallery.Samples.Layout.Stack;

namespace Wice.Samples.Gallery.Samples.Layout;

public class StackSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.LanguageJpn;
    public override string SubTitle => "The Stack visual is used to stack child visuals horizontally or vertically.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new BoxesStackSample();
            yield return new SimpleHorizontalStackSample();
            yield return new SimpleVerticalStackSample();
        }
    }
}
