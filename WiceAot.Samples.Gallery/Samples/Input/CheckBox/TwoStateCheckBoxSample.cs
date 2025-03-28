namespace Wice.Samples.Gallery.Samples.Input.CheckBox;

public class TwoStateCheckBoxSample : Sample
{
    public override string Description => "A 2-state check box";

    public override void Layout(Visual parent)
    {
        // stack checkbox and textbox
        var stack = new Stack { Orientation = Orientation.Horizontal };
        parent.Children.Add(stack);
        Dock.SetDockType(stack, DockType.Top); // remove from display

        // add checkbox
        var cb = new Wice.CheckBox();
        stack.Children.Add(cb);

        // add results textbox
        var results = new TextBox { Margin = D2D_RECT_F.Thickness(10, 0) };
        stack.Children.Add(results);

        cb.Click += (s, e) =>
        {
            results.Text = "CheckBox is " + (cb.Value ? "Checked" : "Unchecked");
        };
    }
}
