namespace Wice.Samples.Gallery.Samples.Collections.CheckBoxList;

public class CheckBoxListSample : Sample
{
    public override string Description => "A simple check box list with a data source composed of strings.";

    public override void Layout(Visual parent)
    {
        var lb = new Wice.CheckBoxList { RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.White.ToColor()) };
        parent.Children.Add(lb);
        Dock.SetDockType(lb, DockType.Top); // remove from display

        // set the list box's data source
        lb.DataSource = new[] { "hello", "happy", "world" };
    }
}
