using Wice.Samples.Gallery.Samples.Media.Image;

namespace Wice.Samples.Gallery.Samples.Media;

public class ImageSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.Picture;
    public override string Description => "The Image visual is used to show and scale images.";
    public override string SubTitle => "A control to display image content.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new BasicImageSample();
        }
    }
}
