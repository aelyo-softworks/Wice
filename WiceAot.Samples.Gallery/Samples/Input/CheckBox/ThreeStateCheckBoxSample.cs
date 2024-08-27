namespace Wice.Samples.Gallery.Samples.Input.CheckBox;

public class ThreeStateCheckBoxSample : Sample
{
    public override int SortOrder => 2;
    public override string Description => "A 3-state check box";

    public override void Layout(Visual parent)
    {
        // stack checkbox and textbox
        var stack = new Stack { Orientation = Orientation.Horizontal };
        parent.Children.Add(stack);
        Dock.SetDockType(stack, DockType.Top); // remove from display

        // add nullable checkbox
        var cb = new NullableCheckBox();
        stack.Children.Add(cb);

        // add results textbox
        var results = new TextBox { Margin = D2D_RECT_F.Thickness(10, 0) };
        stack.Children.Add(results);

        cb.Click += (s, e) =>
        {
            if (cb.Value == null)
            {
                results.Text = "CheckBox state is undetermined.";
            }
            else
            {
                results.Text = "CheckBox is " + (cb.Value.Value ? "Checked" : "Unchecked");
            }
        };
    }
}
