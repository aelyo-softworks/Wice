using Wice.Samples.Gallery.Samples.Effects.Contrast;

namespace Wice.Samples.Gallery.Samples.Effects;

public class ContrastSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Color;
    public override string SubTitle => "Increases or decreases the contrast of an image.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new ContrastSample();
        }
    }
}
