using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Text.RichTextBox
{
    public class SimpleRichTextBoxSample : Sample
    {
        public override string Description => "A simple rich text box.";

        public override void Layout(Visual parent)
        {
            var rtb = new Wice.RichTextBox();
            parent.Children.Add(rtb);
            Dock.SetDockType(rtb, DockType.Top);

            rtb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.GreenYellow.ToColor());
            rtb.Padding = D2D_RECT_F.Thickness(10);
            rtb.Margin = D2D_RECT_F.Thickness(10);
            rtb.Text = "Hello World";
        }
    }
}
