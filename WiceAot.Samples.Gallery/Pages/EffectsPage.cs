using Wice.Samples.Gallery.Samples.Effects;

namespace Wice.Samples.Gallery.Pages;

public partial class EffectsPage : SampleListPage
{
    public EffectsPage()
    {
    }

    public override string IconText => MDL2GlyphResource.MapLayers;
    public override int SortOrder => 8;

    protected override IEnumerable<SampleList> SampleLists
    {
        get
        {
            yield return new ContrastSampleList();
            yield return new GaussianBlurSampleList();
            yield return new GrayscaleSampleList();
            yield return new SepiaSampleList();
            yield return new TemperatureTintSampleList();
        }
    }
}
