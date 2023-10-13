using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Layout.Dock
{
    public class SimpleDockSample : Sample
    {
        public override string Description => "A dock with borders at corners. Note Dock is used in many other samples.";

        public override void Layout(Visual parent)
        {
            var canvas = new Wice.Canvas();
            canvas.Width = 120;
            canvas.Height = 120;
            canvas.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Gray.ToColor());
            parent.Children.Add(canvas);
            Wice.Dock.SetDockType(canvas, DockType.Top); // remove from display

            var b0 = new Wice.Border();
            b0.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red.ToColor());
            b0.Width = 40;
            b0.Height = 40;
            Wice.Canvas.SetLeft(b0, 0);
            Wice.Canvas.SetTop(b0, 0);
            canvas.Children.Add(b0);

            var b1 = new Wice.Border();
            b1.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue.ToColor());
            b1.Width = 40;
            b1.Height = 40;
            Wice.Canvas.SetLeft(b1, 0);
            Wice.Canvas.SetBottom(b1, 0);
            canvas.Children.Add(b1);

            var b2 = new Wice.Border();
            b2.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Green.ToColor());
            b2.Width = 40;
            b2.Height = 40;
            Wice.Canvas.SetRight(b2, 0);
            Wice.Canvas.SetTop(b2, 0);
            canvas.Children.Add(b2);

            var b3 = new Wice.Border();
            b3.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Yellow.ToColor());
            b3.Width = 40;
            b3.Height = 40;
            Wice.Canvas.SetRight(b3, 0);
            Wice.Canvas.SetBottom(b3, 0);
            canvas.Children.Add(b3);
        }
    }
}
