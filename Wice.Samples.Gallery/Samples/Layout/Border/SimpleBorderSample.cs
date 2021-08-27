using DirectN;

namespace Wice.Samples.Gallery.Samples.Layout.Border
{
    public class SimpleBorderSample : Sample
    {
        public override string Description => "A border around a TextBox.";

        public override void Layout(Visual parent)
        {
            var border = new Wice.Border();
            border.BorderThickness = 2;
            border.HorizontalAlignment = Alignment.Near; // force border to use child's size (default is Stretch)
            border.BorderBrush = new SolidColorBrush(new _D3DCOLORVALUE(0xFFFFD700)); // by-value color
            parent.Children.Add(border);
            Wice.Dock.SetDockType(border, DockType.Top); // remove from display

            var textBox = new TextBox();
            textBox.Padding = 10;
            border.Children.Add(textBox);
            textBox.Text = "Text inside a border";
            textBox.FontSize = 18;
        }
    }
}
