namespace Wice.Samples.Gallery.Samples.Layout.ScrollViewer;

public class ScrollViewerBasicSample : Sample
{
    public override string Description => "An image presented in a scroll viewer.";

    public override void Layout(Visual parent)
    {
        // create a 300 pixels height scroll viewer
        var scrollViewer = new Wice.ScrollViewer();
        parent.Children.Add(scrollViewer);
        scrollViewer.Height = 300;
        scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        Wice.Dock.SetDockType(scrollViewer, DockType.Top); // remove from display

        // add an image
        var img = new Image
        {
            InterpolationMode = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_HIGH_QUALITY_CUBIC,

            // load from .NET embedded resource
            Source = Application.CurrentResourceManager.GetWicBitmapSource(Assembly.GetExecutingAssembly(), "Wice.Samples.Gallery.Resources.rainier.jpg")!
        };

        // set the scroll viewer's content
        scrollViewer.Viewer.Child = img;
    }
}
