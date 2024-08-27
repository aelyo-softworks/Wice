namespace Wice.Samples.Gallery.Samples.Input.ToggleSwitch;

public class GroupedToggleSwitchesSample : Sample
{
    public override int SortOrder => 1;
    public override string Description => "A group of toggle switches";

    public override void Layout(Visual parent)
    {
        // dock toggle switches and textbox
        var dock = new Dock();
        parent.Children.Add(dock);
        Dock.SetDockType(dock, DockType.Top); // remove from display

        // add toggle switches
        var list = new List<Wice.ToggleSwitch>();
        for (var i = 0; i < 3; i++)
        {
            var toggle = new Wice.ToggleSwitch { Margin = D2D_RECT_F.Thickness(10, 0), Name = "switch " + i };
            dock.Children.Add(toggle);
            list.Add(toggle);
        }

        // add results textbox
        var results = new TextBox { Margin = D2D_RECT_F.Thickness(10, 0) };
        dock.Children.Add(results);

        // hook click events
        list.ForEach(t => t.Click += (s, e) =>
        {
            var rb = (Wice.ToggleSwitch)s!;
            results.Text = "Selected ToggleSwitch is " + rb.Name;

            // use a method extension to deselect all other buttons
            list.Select(rb);
        });
    }
}
