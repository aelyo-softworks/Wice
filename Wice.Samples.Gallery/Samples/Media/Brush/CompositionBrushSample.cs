using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Media.Brush
{
    public class CompositionBrushSample : Sample
    {
        public override string Description => "A basic composition solid color brush.";

        public override void Layout(Visual parent)
        {
            var box = new Border();
            parent.Children.Add(box);
            Dock.SetDockType(box, DockType.Top); // remove from display
            box.Width = 100;
            box.Height = 100;

            // to create a composition brush, the visual must be attached to composition
            // to be able to use the Compositor instance corresponding to its parent Window.
            box.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.Aquamarine.ToColor());
        }
    }
}
