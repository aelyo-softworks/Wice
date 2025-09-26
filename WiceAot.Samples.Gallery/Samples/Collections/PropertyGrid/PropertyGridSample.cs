namespace Wice.Samples.Gallery.Samples.Collections.PropertyGrid;

public class PropertyGridSample : Sample
{
    public override string Description => "A property grid with a complex object selected. Press Alt+Ctrl-C from a grid value to copy the whole grid into the clipboard.";

    public override void Layout(Visual parent)
    {
        // wrap the property grid in a scroll viewer
        var sv = new ScrollViewer { Width = 800, Height = 500 };
        parent.Children.Add(sv);

#if NETFRAMEWORK // remove from display
        var pg = new Wice.PropertyGrid.PropertyGrid();
#else // remove from display
        var pg = new PropertyGrid<SampleCustomer>(); // remove from display
#endif // remove from display

        pg.CellMargin = 5;
        pg.GroupByCategory = true;
        pg.LiveSync = true;
        sv.Viewer.Child = pg;

        // use a custom complex object for demonstration
        var cus = SampleCustomer.Instance;
        pg.SelectedObject = cus;

        Dock.SetDockType(sv, DockType.Top); // remove from display
    }
}
