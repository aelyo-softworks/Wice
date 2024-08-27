namespace Wice.Samples.Gallery.Samples.Collections.ListBox;

public class AdvancedListBoxSample : Sample
{
    public override string Description => "A list box with custom item content.";

    public override void Layout(Visual parent)
    {
        var lb = new Wice.ListBox
        {
            RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.White.ToColor())
        };
        parent.Children.Add(lb);
        Dock.SetDockType(lb, DockType.Top); // remove from display

        // set the list box's data source
        lb.DataSource = new[] { "hello", "happy", "world" };
    }
}
