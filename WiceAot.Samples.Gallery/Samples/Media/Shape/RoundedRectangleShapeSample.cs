namespace Wice.Samples.Gallery.Samples.Media.Shape;

public class RoundedRectangleShapeSample : Sample
{
    public override string Description => "A rounded rectangle shape.";
    public override int SortOrder => 1;

    public override void Layout(Visual parent)
    {
        var rectangle = new RoundedRectangle
        {
            CornerRadius = new Vector2(parent.Window!.DipsToPixels(50)),
            Width = parent.Window!.DipsToPixels(100),
            Height = parent.Window!.DipsToPixels(100),
            StrokeBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Blue.ToColor()),
            StrokeThickness = parent.Window!.DipsToPixels(20)
        };
        parent.Children.Add(rectangle);
        Dock.SetDockType(rectangle, DockType.Top); // remove from display
    }
}
