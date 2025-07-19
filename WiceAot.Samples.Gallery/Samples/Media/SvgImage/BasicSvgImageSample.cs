namespace Wice.Samples.Gallery.Samples.Media.SvgImage;

public class BasicSvgImageSample : Sample
{
    public override string Description => "A basic SVG image from an embedded file.";

    public override void Layout(Visual parent)
    {
        var img = new Wice.SvgImage();
        parent.Children.Add(img);
        Dock.SetDockType(img, DockType.Top); // remove from display
        img.Width = parent.Window!.DipsToPixels(400);
        img.Height = parent.Window!.DipsToPixels(400);
        img.Margin = parent.Window!.DipsToPixels(10);

        // load from .NET embedded resource
        img.Document = new AssemblyResourceStreamer(Assembly.GetExecutingAssembly(), "Wice.Samples.Gallery.Resources.tiger.svg");
    }
}
