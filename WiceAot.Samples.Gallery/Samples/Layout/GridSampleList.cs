using Wice.Samples.Gallery.Samples.Layout.Grid;

namespace Wice.Samples.Gallery.Samples.Layout;

public class GridSampleList : SampleList
{
    public override bool IsEnabled => false;
    public override string IconText => MDL2GlyphResource.GridView;
    public override string SubTitle => "The Grid visual is used to position content in rows and columns.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new SimpleGridSample();
        }
    }
}
