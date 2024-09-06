using Wice.Samples.Gallery.Samples.Effects.GaussianBlur;

namespace Wice.Samples.Gallery.Samples.Effects;

public class GaussianBlurSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Color;
    public override string SubTitle => "Create a blur based on the Gaussian function over the entire input image.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new GaussianBlurSample();
        }
    }
}
