using Wice.Samples.Gallery.Samples.Misc.ToolTip;

namespace Wice.Samples.Gallery.Samples.Misc;

public class ToolTipSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Message;
    public override string Description => "A ToolTip shows more information about a visual. You might show information about what the visual does, or what the user should do. The ToolTip is shown when a user hovers over a visual.";
    public override string SubTitle => "Displays information for an element in a pop-up window.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new AdvancedToolTipSample();
            yield return new ToolTipSample();
        }
    }
}
