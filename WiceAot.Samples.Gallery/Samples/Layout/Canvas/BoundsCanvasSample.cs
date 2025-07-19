namespace Wice.Samples.Gallery.Samples.Layout.Canvas;

public class BoundsCanvasSample : Sample
{
    public override int SortOrder => 1;
    public override string Description => "A canvas with borders at corners.";

    public override void Layout(Visual parent)
    {
        var canvas = new Wice.Canvas
        {
            Width = parent.Window!.DipsToPixels(120),
            Height = parent.Window!.DipsToPixels(120),
            RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Gray.ToColor())
        };
        parent.Children.Add(canvas);
        Wice.Dock.SetDockType(canvas, DockType.Top); // remove from display

        var b0 = new Wice.Border
        {
            RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Red.ToColor()),
            Width = parent.Window!.DipsToPixels(40),
            Height = parent.Window!.DipsToPixels(40)
        };
        Wice.Canvas.SetLeft(b0, 0);
        Wice.Canvas.SetTop(b0, 0);
        canvas.Children.Add(b0);

        var b1 = new Wice.Border
        {
            RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Blue.ToColor()),
            Width = parent.Window!.DipsToPixels(40),
            Height = parent.Window!.DipsToPixels(40)
        };
        Wice.Canvas.SetLeft(b1, 0);
        Wice.Canvas.SetBottom(b1, 0);
        canvas.Children.Add(b1);

        var b2 = new Wice.Border
        {
            RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Green.ToColor()),
            Width = parent.Window!.DipsToPixels(40),
            Height = parent.Window!.DipsToPixels(40)
        };
        Wice.Canvas.SetRight(b2, 0);
        Wice.Canvas.SetTop(b2, 0);
        canvas.Children.Add(b2);

        var b3 = new Wice.Border
        {
            RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Yellow.ToColor()),
            Width = parent.Window!.DipsToPixels(40),
            Height = parent.Window!.DipsToPixels(40)
        };
        Wice.Canvas.SetRight(b3, 0);
        Wice.Canvas.SetBottom(b3, 0);
        canvas.Children.Add(b3);
    }
}
