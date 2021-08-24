using System;
using System.Numerics;
using DirectN;

namespace Wice.Samples.Gallery.Samples.Layout.Border
{
    public class BordersSample : Sample
    {
        public override int SortOrder => 2;
        public override string Description => "Simple borders in a canvas";

        public override void Layout(Visual parent)
        {
            var canvas = new Canvas();
            canvas.Height = 200; // remove from display
            parent.Children.Add(canvas);
            Dock.SetDockType(canvas, DockType.Top); // remove from display

            var rnd = new Random(Environment.TickCount);
            for (var i = 0; i < 10; i++)
            {
                var box = new Wice.Border();
                // use composition brush
                box.RenderBrush = Compositor.CreateColorBrush(new _D3DCOLORVALUE(rnd.Next(0, int.MaxValue), rnd.NextByte(30)));
                box.Width = rnd.Next(10, 200);
                box.Height = rnd.Next(10, 150);
                canvas.Children.Add(box);
                Canvas.SetLeft(box, rnd.Next(0, 200));
                Canvas.SetTop(box, rnd.Next(0, 100));

                // if you hover the mouse on a box, it's color will change
                box.HoverRenderBrush = Compositor.CreateColorBrush(new _D3DCOLORVALUE(rnd.Next(0, int.MaxValue), rnd.NextByte(30)));
            }
        }
    }
}
