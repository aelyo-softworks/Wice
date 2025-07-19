namespace Wice.Samples.Gallery.Samples.Layout.Border;

public class BordersSample : Sample
{
    public override int SortOrder => 2;
    public override string Description => "Filled borders in a canvas";

    public override void Layout(Visual parent)
    {
#pragma warning disable IDE0017 // Simplify object initialization
        var canvas = new Wice.Canvas();
#pragma warning restore IDE0017 // Simplify object initialization
        canvas.Height = parent.Window!.DipsToPixels(200); // remove from display
        parent.Children.Add(canvas);
        Wice.Dock.SetDockType(canvas, DockType.Top); // remove from display

        var rnd = new Random(Environment.TickCount);
        var widthMin = parent.Window!.DipsToPixels(10);
        var widthMax = parent.Window.DipsToPixels(200);
        var heightMin = parent.Window!.DipsToPixels(10);
        var heightMax = parent.Window.DipsToPixels(150);
        var leftMax = parent.Window.DipsToPixels(200);
        var topMax = parent.Window.DipsToPixels(100);
        for (var i = 0; i < 10; i++)
        {
            var box = new Wice.Border
            {
                // use composition brush
                RenderBrush = Compositor!.CreateColorBrush(new D3DCOLORVALUE(rnd.Next(0, int.MaxValue), rnd.NextByte(30)).ToColor()),
                Width = rnd.Next(widthMin, widthMax),
                Height = rnd.Next(heightMin, heightMax)
            };
            canvas.Children.Add(box);
            Wice.Canvas.SetLeft(box, rnd.Next(0, leftMax));
            Wice.Canvas.SetTop(box, rnd.Next(0, topMax));

            // if you hover the mouse on a box, it's color will change
            box.HoverRenderBrush = Compositor.CreateColorBrush(new D3DCOLORVALUE(rnd.Next(0, int.MaxValue), rnd.NextByte(30)).ToColor());
        }
    }
}
