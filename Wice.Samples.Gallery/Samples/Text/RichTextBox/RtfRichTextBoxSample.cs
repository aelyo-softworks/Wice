using DirectN;

namespace Wice.Samples.Gallery.Samples.Text.RichTextBox
{
    public class RtfRichTextBoxSample : Sample
    {
        public override string Description => "A rich text box filled from an RTF string.";
        public override int SortOrder => 2;

        public override void Layout(Visual parent)
        {
            var rtb = new Wice.RichTextBox();
            parent.Children.Add(rtb);
            Dock.SetDockType(rtb, DockType.Top);

            rtb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White);
            rtb.Padding = D2D_RECT_F.Thickness(10);
            rtb.Margin = D2D_RECT_F.Thickness(10);

            rtb.RtfText = @"{\rtf1\ansi\deff0
                {\colortbl;\red0\green0\blue0;\red255\green0\blue0;}
                This line is the default color\line
                \cf2
                This line is red with special characters: éèà 😱 \line
                \cf1
                This line is the default color
                }";
        }
    }
}
