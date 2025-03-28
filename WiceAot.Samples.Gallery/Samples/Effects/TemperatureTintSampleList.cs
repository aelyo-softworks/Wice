using Wice.Samples.Gallery.Samples.Effects.TemperatureTint;

namespace Wice.Samples.Gallery.Samples.Effects;

public class TemperatureTintSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Color;
    public override string SubTitle => "Changes the input image temperature and tint.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new TemperatureTintSample();
        }
    }
}
