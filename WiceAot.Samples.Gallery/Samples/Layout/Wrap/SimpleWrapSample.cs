namespace Wice.Samples.Gallery.Samples.Layout.Wrap;

public class SimpleWrapSample : Sample
{
    public override string Description => "An horizontal wrap with around 100 child visuals. You can resize the window to see the wrapping effect.";

    public override void Layout(Visual parent)
    {
        var wrap = new Wice.Wrap { Orientation = Orientation.Horizontal };
        parent.Children.Add(wrap);
        Wice.Dock.SetDockType(wrap, DockType.Top); // remove from display

        var rnd = new Random(Environment.TickCount);
        var max = 100;
        var rndMin = parent.Window!.DipsToPixels(10);
        var rndMax = parent.Window.DipsToPixels(60);
        for (var i = 0; i < max; i++)
        {
            var border = new Wice.Border();
            wrap.Children.Add(border);
            var color = D3DCOLORVALUE.FromArgb(rnd.NextByte(), rnd.NextByte(), rnd.NextByte());
            border.RenderBrush = Compositor!.CreateColorBrush(color.ToColor());
            border.Width = rnd.Next(rndMin, rndMax);
            border.Height = rnd.Next(rndMin, rndMax);
        }
    }
}
