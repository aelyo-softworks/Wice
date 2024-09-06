using Wice.Samples.Gallery.Samples.Misc.Focus;

namespace Wice.Samples.Gallery.Samples.Misc;

public class FocusSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Keyboard;
    public override string SubTitle => "This section details how Wice Focus works.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new FocusSample();
        }
    }
}
