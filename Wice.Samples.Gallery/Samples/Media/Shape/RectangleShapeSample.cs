using DirectN;

namespace Wice.Samples.Gallery.Samples.Media.Shape
{
    public class RectangleShapeSample : Sample
    {
        public override string Description => "A simple rectangle shape.";

        public override void Layout(Visual parent)
        {
            var rectangle = new Rectangle();
            rectangle.Width = 200;
            rectangle.Height = 100;
            rectangle.StrokeBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Red);
            rectangle.StrokeThickness = 10;
            parent.Children.Add(rectangle);
            Dock.SetDockType(rectangle, DockType.Top); // remove from display
        }
    }
}
