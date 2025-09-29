using Wice.Samples.Gallery.Samples.Collections.PropertyGrid;

namespace Wice.Samples.Gallery.Samples.Collections;

public class PropertyGridSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Equalizer;
    public override string SubTitle => "A visual that provides a user interface for automatically browsing the properties of an object.";
    public override string Description => SubTitle + " Press Alt+Ctrl-C from a grid value to copy the whole grid's texts into the clipboard.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new PropertyGridSample();
        }
    }
}
