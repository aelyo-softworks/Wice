using Wice.Samples.Gallery.Samples.Misc.TitleBar;

namespace Wice.Samples.Gallery.Samples.Misc;

public class TitleBarSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Favicon;
    public override string SubTitle => "The TitleBar visual can be used to replace the system's titlebar or caption.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new TitleBarSample();
        }
    }
}
