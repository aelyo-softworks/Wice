using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Text.RichTextBox
{
    public class RtfRichTextBoxSample : RichTextBoxSample
    {
        public override string Description => "A rich text box filled from an RTF string.";
        public override int SortOrder => 2;

        public override void Layout(Visual parent)
        {
            parent.Children.Add(Rtb);
            Dock.SetDockType(Rtb, DockType.Top);

            Rtb.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White.ToColor());
            Rtb.Padding = D2D_RECT_F.Thickness(10);
            Rtb.Margin = D2D_RECT_F.Thickness(10);

            Rtb.RtfText = @"{\rtf1\ansi\deff0
                {\coloRtbl;\red0\green0\blue0;\red255\green0\blue0;}
                This line is the default color\line
                \cf2
                This line is red with special characters: éèà 😱 \line
                \cf1
                This line is the default color
                }";
        }
    }
}
