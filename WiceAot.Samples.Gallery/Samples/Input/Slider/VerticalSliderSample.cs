namespace Wice.Samples.Gallery.Samples.Input.Slider;

public class VerticalSliderSample : Sample
{
    public override string Description => "Two vertical sliders (Double and Decimal) with ticks values. The second one's texts are vertical";

    public override void Layout(Visual parent)
    {
        // stack radio button and textbox
        var stack = new Stack
        {
            Orientation = Orientation.Horizontal,
            Spacing = new D2D_SIZE_F(20, 20)
        };
        parent.Children.Add(stack);
        Dock.SetDockType(stack, DockType.Top); // remove from display

        // add vertical slider for Decimal
        var slider1 = new Slider<decimal>
        {
            Height = parent.Window!.DipsToPixels(400),
            Orientation = Orientation.Vertical,

            // change range
            MinValue = 1000,
            MaxValue = 10000,
            TicksStep = 1000,
        };
        slider1.TicksOptions |= SliderTicksOptions.ShowTickValues;
        stack.Children.Add(slider1);

        // add vertical slider for Single
        var slider2 = new Slider<double>
        {
            Height = parent.Window!.DipsToPixels(400),
            Orientation = Orientation.Vertical
        };
        slider2.TicksOptions |= SliderTicksOptions.ShowTickValues;
        slider2.TextOrientation = Orientation.Vertical;
        stack.Children.Add(slider2);
    }
}
