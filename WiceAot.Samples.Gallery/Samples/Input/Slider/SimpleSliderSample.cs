namespace Wice.Samples.Gallery.Samples.Input.Slider;

public class SimpleSliderSample : Sample
{
    public override string Description => "A simple Int32 slider";

    public override void Layout(Visual parent)
    {
        // stack radio button and textbox
        var stack = new Stack { Orientation = Orientation.Horizontal };
        parent.Children.Add(stack);
        Dock.SetDockType(stack, DockType.Top); // remove from display

        // add slider for Int32
        var slider = new Slider<int>();
        slider.Width = parent.Window!.DipsToPixels(400);
        stack.Children.Add(slider);

        // add results textbox
        var results = new TextBox { Margin = D2D_RECT_F.Thickness(parent.Window!.DipsToPixels(10), 0) };
        stack.Children.Add(results);

        slider.ValueChanged += (s, e) => { results.Text = "Value is " + slider.Value; };
    }
}
