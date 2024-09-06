using Wice.Samples.Gallery.Samples.Layout.Dock;

namespace Wice.Samples.Gallery.Samples.Layout;

public class DockSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.ViewDashboard;
    public override string Description => "Use a DockPanel to arrange child visual relative to each other, either horizontally or vertically. With Dock you can easily dock child visual to top, bottom, right, left using the DockType property.";
    public override string SubTitle => "The Dock visual is used to position child content along the edge of a layout container.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new SimpleDockSample();
        }
    }
}
