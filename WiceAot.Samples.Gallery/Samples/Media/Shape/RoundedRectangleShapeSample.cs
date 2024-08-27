namespace Wice.Samples.Gallery.Samples.Media.Shape;

public class RoundedRectangleShapeSample : Sample
{
    public override string Description => "A rounded rectangle shape.";
    public override int SortOrder => 1;

    public override void Layout(Visual parent)
    {
        var rectangle = new RoundedRectangle
        {
            CornerRadius = new Vector2(50),
            Width = 100,
            Height = 100,
            StrokeBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Blue.ToColor()),
            StrokeThickness = 20
        };
        parent.Children.Add(rectangle);
        Dock.SetDockType(rectangle, DockType.Top); // remove from display
    }
}
