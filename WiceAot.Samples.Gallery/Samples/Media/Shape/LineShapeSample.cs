namespace Wice.Samples.Gallery.Samples.Media.Shape;

public class LineShapeSample : Sample
{
    public override string Description => "Two line shapes.";
    public override int SortOrder => 3;

    public override void Layout(Visual parent)
    {
        var line1 = new Line
        {
            Width = parent.Window!.DipsToPixels(200),
            Height = parent.Window!.DipsToPixels(100),
            StrokeBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Maroon.ToColor()),
            StrokeThickness = parent.Window!.DipsToPixels(1)
        };
        parent.Children.Add(line1);
        line1.Arranged += (s, e) =>
        {
            line1.Geometry!.End = line1.ArrangedRect.Size.ToVector2();
        };
        Dock.SetDockType(line1, DockType.Top); // remove from display

        var line2 = new Line
        {
            Width = parent.Window!.DipsToPixels(200),
            Height = parent.Window!.DipsToPixels(100),
            StrokeBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.BlueViolet.ToColor()),
            StrokeThickness = parent.Window!.DipsToPixels(4)
        };
        parent.Children.Add(line2);
        line2.Arranged += (s, e) =>
        {
            var ar = line2.ArrangedRect.Size;
            line2.Geometry!.Start = new Vector2(0, ar.height);
            line2.Geometry.End = new Vector2(ar.width, 0);
        };
        Dock.SetDockType(line2, DockType.Top);
    }
}
