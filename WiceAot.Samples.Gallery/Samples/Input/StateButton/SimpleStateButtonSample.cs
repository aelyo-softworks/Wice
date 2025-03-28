namespace Wice.Samples.Gallery.Samples.Input.StateButton;

public class SimpleStateButtonSample : Sample
{
    public override string Description => "A 4-state button";

    public override void Layout(Visual parent)
    {
        // stack state button and textbox
        var stack = new Stack { Orientation = Orientation.Horizontal };
        parent.Children.Add(stack);
        Dock.SetDockType(stack, DockType.Top); // remove from display

        // add state button
        var btn = new Wice.StateButton { Width = 100, Height = 100 };
        for (var i = 0; i < 4; i++)
        {
            // second parameter defines the child visual, here just a textbox
            btn.AddState(new StateButtonState("state " + i, (button, args, state) =>
            {
                var tb = new TextBox
                {
                    Text = state.ToString(),
                    HorizontalAlignment = Alignment.Center,
                    VerticalAlignment = Alignment.Center
                };
                return tb;
            }));
        }

        // select one value
        btn.Value = "state 0";
        btn.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.OldLace.ToColor());
        stack.Children.Add(btn);

        // add results textbox
        var results = new TextBox { Margin = D2D_RECT_F.Thickness(10, 0), HorizontalAlignment = Alignment.Center, VerticalAlignment = Alignment.Center };
        stack.Children.Add(results);

        btn.Click += (s, e) => { results.Text = "StateButton value is " + btn.Value; };
    }
}
