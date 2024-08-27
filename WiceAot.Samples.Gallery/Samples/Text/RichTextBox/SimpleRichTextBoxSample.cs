namespace Wice.Samples.Gallery.Samples.Text.RichTextBox;

public partial class SimpleRichTextBoxSample : RichTextBoxSample
{
    public override string Description => "A simple rich text box.";

    public override void Layout(Visual parent)
    {
        parent.Children.Add(Rtb);
        Dock.SetDockType(Rtb, DockType.Top);

        Rtb.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.GreenYellow.ToColor());
        Rtb.Padding = D2D_RECT_F.Thickness(10);
        Rtb.Margin = D2D_RECT_F.Thickness(10);
        Rtb.Text = "Hello World";
    }
}
