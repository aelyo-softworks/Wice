namespace Wice.Samples.Gallery.Samples.Collections.PropertyGrid;

public class PropertyGridSample : Sample
{
    public override string Description => "A property grid with a complex object selected.";

    public override void Layout(Visual parent)
    {
        // wrap the property grid in a scroll viewer
        var sv = new ScrollViewer
        {
            Height = 500,
            Width = 500
        };
        parent.Children.Add(sv);

        var pg = new Wice.PropertyGrid.PropertyGrid
        {
            CellMargin = 5
        };
        sv.Viewer.Child = pg;

        // use a custom complex object for demonstration
        var cus = new SampleCustomer();
        pg.SelectedObject = cus;

        Dock.SetDockType(sv, DockType.Top); // remove from display
    }
}
