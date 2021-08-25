using DirectN;

namespace Wice.Samples.Gallery.Samples.Windows
{
    public class WindowSampleList : SampleList
    {
        public override string IconText => MDL2GlyphResource.Pictures;
        public override string Description => "A Window is also a Canvas visual that contain all other visuals of your app, including and popups and dialogs. An application can have more than one Window.";
        public override string SubTitle => "The first and main Visual of your app. Associated with a Win32 window with its handle. Other Windows can be created in an Application.";
    }
}
