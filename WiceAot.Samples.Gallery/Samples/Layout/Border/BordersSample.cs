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
        canvas.Height = 200; // remove from display
        parent.Children.Add(canvas);
        Wice.Dock.SetDockType(canvas, DockType.Top); // remove from display

        var rnd = new Random(Environment.TickCount);
        for (var i = 0; i < 10; i++)
        {
            var box = new Wice.Border
            {
                // use composition brush
                RenderBrush = Compositor!.CreateColorBrush(new D3DCOLORVALUE(rnd.Next(0, int.MaxValue), rnd.NextByte(30)).ToColor()),
                Width = rnd.Next(10, 200),
                Height = rnd.Next(10, 150)
            };
            canvas.Children.Add(box);
            Wice.Canvas.SetLeft(box, rnd.Next(0, 200));
            Wice.Canvas.SetTop(box, rnd.Next(0, 100));

            // if you hover the mouse on a box, it's color will change
            box.HoverRenderBrush = Compositor.CreateColorBrush(new D3DCOLORVALUE(rnd.Next(0, int.MaxValue), rnd.NextByte(30)).ToColor());
        }
    }
}
