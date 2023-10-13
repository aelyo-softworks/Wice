using System;
using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Layout.Wrap
{
    public class SimpleWrapSample : Sample
    {
        public override string Description => "An horizontal wrap with around 100 child visuals. You can resize the window to see the wrapping effect.";

        public override void Layout(Visual parent)
        {
            var wrap = new Wice.Wrap();
            wrap.Orientation = Orientation.Horizontal;
            parent.Children.Add(wrap);
            Wice.Dock.SetDockType(wrap, DockType.Top); // remove from display

            var rnd = new Random(Environment.TickCount);
            var max = 100;
            for (var i = 0; i < max; i++)
            {
                var border = new Wice.Border();
                wrap.Children.Add(border);
                var color = _D3DCOLORVALUE.FromArgb(rnd.NextByte(), rnd.NextByte(), rnd.NextByte());
                border.RenderBrush = Compositor.CreateColorBrush(color.ToColor());
                border.Width = rnd.Next(10, 60);
                border.Height = rnd.Next(10, 60);
            }
        }
    }
}
