namespace Wice.Samples.Gallery.Samples.Media.Image;

public class BasicImageSample : Sample
{
    public override string Description => "A basic image from an embedded file.";

    public override void Layout(Visual parent)
    {
        var img = new Wice.Image();
        parent.Children.Add(img);
        Dock.SetDockType(img, DockType.Top); // remove from display
        img.Width = 400;
        img.Margin = 10;
        img.InterpolationMode = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_HIGH_QUALITY_CUBIC;

        // load from .NET embedded resource
        img.Source = Application.Current.ResourceManager.GetWicBitmapSource(Assembly.GetExecutingAssembly(), "Wice.Samples.Gallery.Resources.rainier.jpg")!;
    }
}
