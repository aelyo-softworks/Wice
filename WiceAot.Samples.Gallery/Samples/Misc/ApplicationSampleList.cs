using Wice.Samples.Gallery.Samples.Misc.Application;

namespace Wice.Samples.Gallery.Samples.Misc;

public class ApplicationSampleList : SampleList
{
    public override string IconText => MDL2GlyphResource.PreviewLink;
    public override string SubTitle => "Provides methods and properties to manage a Wice application. The Application contains the main and other windows.";
    public override string Description => "The Application class provides methods and properties to manage an application, such as methods to start and stop an application, and properties to get information about an application.";

    protected override IEnumerable<Sample> Types
    {
        get
        {
            yield return new ApplicationSample();
            yield return new MultipleApplicationSample();
        }
    }
}
