namespace Wice.Samples.Gallery.Samples.Layout.Canvas;

public class SimpleCanvasSample : Sample
{
    public override string Description => "A canvas with filled borders.";

    public override void Layout(Visual parent)
    {
        var canvas = new Wice.Canvas
        {
            Width = 120,
            Height = 120,
            RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Gray.ToColor())
        };
        parent.Children.Add(canvas);
        Wice.Dock.SetDockType(canvas, DockType.Top); // remove from display

        var b0 = new Wice.Border
        {
            RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Red.ToColor()),
            Width = 40,
            Height = 40
        };
        Wice.Canvas.SetLeft(b0, 0);
        Wice.Canvas.SetTop(b0, 0);
        canvas.Children.Add(b0);

        var b1 = new Wice.Border
        {
            RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Blue.ToColor()),
            Width = 40,
            Height = 40
        };
        Wice.Canvas.SetLeft(b1, 20);
        Wice.Canvas.SetTop(b1, 20);
        canvas.Children.Add(b1);

        var b2 = new Wice.Border
        {
            RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Green.ToColor()),
            Width = 40,
            Height = 40
        };
        Wice.Canvas.SetLeft(b2, 40);
        Wice.Canvas.SetTop(b2, 40);
        canvas.Children.Add(b2);

        var b3 = new Wice.Border
        {
            RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Yellow.ToColor()),
            Width = 40,
            Height = 40
        };
        Wice.Canvas.SetLeft(b3, 60);
        Wice.Canvas.SetTop(b3, 60);
        canvas.Children.Add(b3);
    }
}
