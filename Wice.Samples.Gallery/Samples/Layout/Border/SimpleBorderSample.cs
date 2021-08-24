using DirectN;

namespace Wice.Samples.Gallery.Samples.Layout.Border
{
    public class SimpleBorderSample : Sample
    {
        public override int SortOrder => 0;
        public override string Description => "A border around a TextBox.";

        public override void Layout(Visual parentVisual)
        {
            var border = new Wice.Border();
            border.BorderThickness = 2;
            border.HorizontalAlignment = Alignment.Near;
            border.BorderBrush = new SolidColorBrush(new _D3DCOLORVALUE(0xFFFFD700));
            parentVisual.Children.Add(border);
            Dock.SetDockType(border, DockType.Top);

            var textBox = new TextBox();
            textBox.Padding = 10;
            border.Children.Add(textBox);
            textBox.Text = "Text inside a border";
            textBox.FontSize = 18;
        }
    }
}
