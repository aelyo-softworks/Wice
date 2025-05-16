using Wice.Samples.Gallery.Samples.Layout.Tabs;

namespace Wice.Samples.Gallery.Samples.Layout;

public class TabsSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.LanguageJpn;
    public override string SubTitle => "The Tabs visual contains a collection of TabPage intsances to display several \"content\" visuals.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new TabsSample();
        }
    }
}
