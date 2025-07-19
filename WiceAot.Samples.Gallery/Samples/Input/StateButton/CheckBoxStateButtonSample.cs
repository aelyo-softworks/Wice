namespace Wice.Samples.Gallery.Samples.Input.StateButton;

public class CheckBoxStateButtonSample : Sample
{
    public override int SortOrder => 1;
    public override string Description => "A checkbox recreated using a state button";

    public override void Layout(Visual parent)
    {
        // stack state button and textbox
        var stack = new Stack { Orientation = Orientation.Horizontal };
        parent.Children.Add(stack);
        Dock.SetDockType(stack, DockType.Top); // remove from display

        // add state button
        var btn = new Wice.StateButton { RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Blue.ToColor()) };

        // add "true" / checked state
        btn.AddState(new StateButtonState(true, (button, args, state) => new TextBox
        {
            ForegroundBrush = new SolidColorBrush(D3DCOLORVALUE.White),
            Text = "X",
            VerticalAlignment = Alignment.Center,
            HorizontalAlignment = Alignment.Center
        })); ;

        // add "false" / unchecked set
        btn.AddState(new StateButtonState(false, (button, args, state) => new TextBox()));
        btn.Width = parent.Window!.DipsToPixels(20);
        btn.Height = parent.Window!.DipsToPixels(20);

        // add results textbox
        var results = new TextBox { Margin = D2D_RECT_F.Thickness(parent.Window!.DipsToPixels(10), 0), HorizontalAlignment = Alignment.Center, VerticalAlignment = Alignment.Center };
        stack.Children.Add(results);

        stack.Children.Add(btn);
        btn.Click += (s, e) => { results.Text = "\"CheckBox\" StateButton is " + (((bool)btn.Value!) ? "Checked" : "Unchecked"); };
    }
}
