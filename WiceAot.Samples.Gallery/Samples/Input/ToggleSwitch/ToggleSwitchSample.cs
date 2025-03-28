namespace Wice.Samples.Gallery.Samples.Input.ToggleSwitch;

public class ToggleSwitchSample : Sample
{
    public override string Description => "A toggle switch button";

    public override void Layout(Visual parent)
    {
        // stack state button and textbox
        var stack = new Stack
        {
            Orientation = Orientation.Horizontal
        };
        parent.Children.Add(stack);
        Dock.SetDockType(stack, DockType.Top); // remove from display

        // add toggle switch button
        var toggle = new Wice.ToggleSwitch();
        stack.Children.Add(toggle);

        // add results textbox
        var results = new TextBox
        {
            Margin = D2D_RECT_F.Thickness(10, 0),
            HorizontalAlignment = Alignment.Center,
            VerticalAlignment = Alignment.Center
        };
        stack.Children.Add(results);

        toggle.Click += (s, e) =>
        {
            results.Text = "ToggleSwitch is " + (toggle.Value ? "Checked" : "Unchecked");
        };
    }
}
