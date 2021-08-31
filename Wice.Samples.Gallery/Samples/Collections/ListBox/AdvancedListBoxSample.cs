using DirectN;

namespace Wice.Samples.Gallery.Samples.Collections.ListBox
{
    public class AdvancedListBoxSample : Sample
    {
        public override string Description => "A list box with custom item content.";

        public override void Layout(Visual parent)
        {
            var lb = new Wice.ListBox();
            lb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White);
            parent.Children.Add(lb);
            Dock.SetDockType(lb, DockType.Top); // remove from display

            // set the list box's data source
            lb.DataSource = new[] { "hello", "happy", "world" };
        }
    }
}
