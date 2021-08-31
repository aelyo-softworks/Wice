using DirectN;

namespace Wice.Samples.Gallery.Samples.Text.TextBox
{
    public class ColorFontTextBoxSample : Sample
    {
        public override string Description => "A non editable text box that demonstrates the color fonts.";
        public override int SortOrder => 2;

        public override void Layout(Visual parent)
        {
            var tb = new Wice.TextBox();
            parent.Children.Add(tb);
            Dock.SetDockType(tb, DockType.Top);

            tb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Pink);
            tb.Padding = D2D_RECT_F.Thickness(10);
            tb.Margin = D2D_RECT_F.Thickness(10);

            // note the difference between the source code (created with Windows' emoji editor) and the rendered text.
            tb.Text = "These are colored emoji: 😝👸🎅👨‍👩‍👧‍👦";
        }
    }
}
