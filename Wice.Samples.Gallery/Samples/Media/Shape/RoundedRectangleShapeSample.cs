using DirectN;

namespace Wice.Samples.Gallery.Samples.Media.Shape
{
    public class RoundedRectangleShapeSample : Sample
    {
        public override string Description => "A rounded rectangle shape.";
        public override int SortOrder => 1;

        public override void Layout(Visual parent)
        {
            var rectangle = new RoundedRectangle();
            rectangle.CornerRadius = new System.Numerics.Vector2(50);
            rectangle.Width = 100;
            rectangle.Height = 100;
            rectangle.StrokeBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Blue);
            rectangle.StrokeThickness = 20;
            parent.Children.Add(rectangle);
            Dock.SetDockType(rectangle, DockType.Top); // remove from display
        }
    }
}
