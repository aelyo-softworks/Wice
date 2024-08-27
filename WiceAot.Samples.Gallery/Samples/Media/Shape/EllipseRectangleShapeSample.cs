namespace Wice.Samples.Gallery.Samples.Media.Shape;

public class EllipseRectangleShapeSample : Sample
{
    public override string Description => "An ellipse shape.";
    public override int SortOrder => 2;

    public override void Layout(Visual parent)
    {
        var ellipse = new Ellipse
        {
            Width = 300,
            Height = 100,
            Radius = new Vector2(150, 50),
            StrokeBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Green.ToColor()),
            StrokeThickness = 2,
            StrokeDashArray = [1, 2, 3]
        };
        parent.Children.Add(ellipse);
        Dock.SetDockType(ellipse, DockType.Top); // remove from display
    }
}
