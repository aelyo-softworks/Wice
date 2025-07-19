namespace Wice.Samples.Gallery.Samples.Media.Shape;

public class RectangleShapeSample : Sample
{
    public override string Description => "A simple rectangle shape.";

    public override void Layout(Visual parent)
    {
        var rectangle = new Rectangle
        {
            Width = parent.Window!.DipsToPixels(200),
            Height = parent.Window!.DipsToPixels(100),
            StrokeBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Red.ToColor()),
            StrokeThickness = parent.Window!.DipsToPixels(10)
        };
        parent.Children.Add(rectangle);
        Dock.SetDockType(rectangle, DockType.Top); // remove from display
    }
}
