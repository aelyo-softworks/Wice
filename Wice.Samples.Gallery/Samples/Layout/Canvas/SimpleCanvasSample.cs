using DirectN;

namespace Wice.Samples.Gallery.Samples.Layout.Canvas
{
    public class SimpleCanvasSample : Sample
    {
        public override string Description => "A canvas with filled borders.";

        public override void Layout(Visual parent)
        {
            var canvas = new Wice.Canvas();
            canvas.Width = 120;
            canvas.Height = 120;
            canvas.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Gray);
            parent.Children.Add(canvas);
            Wice.Dock.SetDockType(canvas, DockType.Top); // remove from display

            var b0 = new Wice.Border();
            b0.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);
            b0.Width = 40;
            b0.Height = 40;
            Wice.Canvas.SetLeft(b0, 0);
            Wice.Canvas.SetTop(b0, 0);
            canvas.Children.Add(b0);

            var b1 = new Wice.Border();
            b1.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue);
            b1.Width = 40;
            b1.Height = 40;
            Wice.Canvas.SetLeft(b1, 20);
            Wice.Canvas.SetTop(b1, 20);
            canvas.Children.Add(b1);

            var b2 = new Wice.Border();
            b2.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Green);
            b2.Width = 40;
            b2.Height = 40;
            Wice.Canvas.SetLeft(b2, 40);
            Wice.Canvas.SetTop(b2, 40);
            canvas.Children.Add(b2);

            var b3 = new Wice.Border();
            b3.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Yellow);
            b3.Width = 40;
            b3.Height = 40;
            Wice.Canvas.SetLeft(b3, 60);
            Wice.Canvas.SetTop(b3, 60);
            canvas.Children.Add(b3);
        }
    }
}
