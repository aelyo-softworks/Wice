using System.Numerics;
using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Media.Shape
{
    public class LineShapeSample : Sample
    {
        public override string Description => "Two line shapes.";
        public override int SortOrder => 3;

        public override void Layout(Visual parent)
        {
            var line1 = new Line();
            line1.Width = 200;
            line1.Height = 100;
            line1.StrokeBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Maroon.ToColor());
            line1.StrokeThickness = 1;
            parent.Children.Add(line1);
            line1.Arranged += (s, e) =>
            {
                line1.Geometry.End = Wice.Utilities.Extensions.ToVector2(line1.ArrangedRect.Size);
            };
            Dock.SetDockType(line1, DockType.Top); // remove from display

            var line2 = new Line();
            line2.Width = 200;
            line2.Height = 100;
            line2.StrokeBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.BlueViolet.ToColor());
            line2.StrokeThickness = 4;
            parent.Children.Add(line2);
            line2.Arranged += (s, e) =>
            {
                var ar = line2.ArrangedRect.Size;
                line2.Geometry.Start = new Vector2(0, ar.height);
                line2.Geometry.End = new Vector2(ar.width, 0);
            };
            Dock.SetDockType(line2, DockType.Top);
        }
    }
}
