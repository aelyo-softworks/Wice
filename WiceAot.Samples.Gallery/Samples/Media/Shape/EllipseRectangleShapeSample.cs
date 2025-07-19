namespace Wice.Samples.Gallery.Samples.Media.Shape;

public class EllipseRectangleShapeSample : Sample
{
    public override string Description => "An ellipse shape.";
    public override int SortOrder => 2;

    public override void Layout(Visual parent)
    {
        var ellipse = new Ellipse
        {
            Width = parent.Window!.DipsToPixels(300),
            Height = parent.Window!.DipsToPixels(100),
            Radius = new Vector2(parent.Window!.DipsToPixels(150), parent.Window!.DipsToPixels(50)),
            StrokeBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Green.ToColor()),
            StrokeThickness = parent.Window!.DipsToPixels(2),
            StrokeDashArray = [1, 2, 3]
        };
        parent.Children.Add(ellipse);
        Dock.SetDockType(ellipse, DockType.Top); // remove from display
    }
}
