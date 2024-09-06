using Wice.Samples.Gallery.Samples.Effects.Grayscale;

namespace Wice.Samples.Gallery.Samples.Effects;

public class GrayscaleSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Color;
    public override string SubTitle => "Converts an image to monochromatic gray.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new GrayscaleSample();
        }
    }
}
