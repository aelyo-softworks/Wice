using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Media.Shape
{
    public class EllipseRectangleShapeSample : Sample
    {
        public override string Description => "An ellipse shape.";
        public override int SortOrder => 2;

        public override void Layout(Visual parent)
        {
            var ellipse = new Ellipse();
            ellipse.Width = 300;
            ellipse.Height = 100;
            ellipse.Radius = new System.Numerics.Vector2(150, 50);
            ellipse.StrokeBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Green.ToColor());
            ellipse.StrokeThickness = 2;
            ellipse.StrokeDashArray = new float[] { 1, 2, 3 };
            parent.Children.Add(ellipse);
            Dock.SetDockType(ellipse, DockType.Top); // remove from display
        }
    }
}
