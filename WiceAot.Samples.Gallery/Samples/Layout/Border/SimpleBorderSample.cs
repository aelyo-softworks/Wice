namespace Wice.Samples.Gallery.Samples.Layout.Border;

public class SimpleBorderSample : Sample
{
    public override string Description => "A border around a TextBox.";

    public override void Layout(Visual parent)
    {
        var border = new Wice.Border
        {
            BorderThickness = 2,
            HorizontalAlignment = Alignment.Near, // force border to use child's size (default is Stretch)
            BorderBrush = new SolidColorBrush(new D3DCOLORVALUE(0xFFFFD700)) // by-value color
        };
        parent.Children.Add(border);
        Wice.Dock.SetDockType(border, DockType.Top); // remove from display

        var textBox = new TextBox
        {
            Padding = 10
        };
        border.Children.Add(textBox);
        textBox.Text = "Text inside a border";
        textBox.FontSize = 18;
    }
}
