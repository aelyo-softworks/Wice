namespace Wice.Samples.Gallery.Samples.Collections.PropertyGrid;

public class PropertyGridSample : Sample
{
    public override string Description => "A property grid with a complex object selected.";

    public override void Layout(Visual parent)
    {
        // wrap the property grid in a scroll viewer
        var sv = new ScrollViewer { Width = 800, Height = 500 };
        parent.Children.Add(sv);

#if NETFRAMEWORK // remove from display
        var pg = new Wice.PropertyGrid.PropertyGrid
        {
            CellMargin = 5
        };
#else // remove from display
        var pg = new Wice.PropertyGrid.PropertyGrid<SampleCustomer> // remove from display
        {
            CellMargin = 5 // remove from display
        }; // remove from display
#endif // remove from display
        sv.Viewer.Child = pg;

        // use a custom complex object for demonstration
        var cus = SampleCustomer.Instance;
        pg.SelectedObject = cus;

        Dock.SetDockType(sv, DockType.Top); // remove from display
    }
}
