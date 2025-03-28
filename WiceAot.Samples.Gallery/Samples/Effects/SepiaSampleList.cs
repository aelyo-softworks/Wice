using Wice.Samples.Gallery.Samples.Effects.Sepia;

namespace Wice.Samples.Gallery.Samples.Effects;

public class SepiaSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Color;
    public override string SubTitle => "Converts an image to sepia tones.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new SepiaSample();
        }
    }
}
