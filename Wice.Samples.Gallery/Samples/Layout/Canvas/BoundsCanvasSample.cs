using DirectN;

namespace Wice.Samples.Gallery.Samples.Layout.Canvas
{
    public class BoundsCanvasSample : Sample
    {
        public override int SortOrder => 1;
        public override string Description => "A canvas with borders at corners.";

        public override void Layout(Visual parent)
        {
            var canvas = new Wice.Canvas();
            canvas.Width = 120;
            canvas.Height = 120;
            canvas.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Gray);
            parent.Children.Add(canvas);
            Dock.SetDockType(canvas, DockType.Top); // remove from display

            var b0 = new Wice.Border();
            b0.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);
            b0.Width= 40;
            b0.Height= 40;
            Wice.Canvas.SetLeft(b0, 0);
            Wice.Canvas.SetTop(b0, 0);
            canvas.Children.Add(b0);

            var b1 = new Wice.Border();
            b1.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue);
            b1.Width= 40;
            b1.Height= 40;
            Wice.Canvas.SetLeft(b1, 0);
            Wice.Canvas.SetBottom(b1, 0);
            canvas.Children.Add(b1);

            var b2 = new Wice.Border();
            b2.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Green);
            b2.Width= 40;
            b2.Height= 40;
            Wice.Canvas.SetRight(b2, 0);
            Wice.Canvas.SetTop(b2, 0);
            canvas.Children.Add(b2);

            var b3 = new Wice.Border();
            b3.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Yellow);
            b3.Width= 40;
            b3.Height= 40;
            Wice.Canvas.SetRight(b3, 0);
            Wice.Canvas.SetBottom(b3, 0);
            canvas.Children.Add(b3);
        }
    }
}
