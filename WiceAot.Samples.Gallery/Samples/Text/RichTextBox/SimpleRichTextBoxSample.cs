namespace Wice.Samples.Gallery.Samples.Text.RichTextBox;

public partial class SimpleRichTextBoxSample : Sample
{
    public override string Description => "A simple rich text box.";

    public override void Layout(Visual parent)
    {
        var rtb = new Wice.RichTextBox();
        rtb.FontSize = parent.Window!.DipsToPixels(10);
        parent.Children.Add(rtb);
        Dock.SetDockType(rtb, DockType.Top);

        rtb.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.GreenYellow.ToColor());
        rtb.Padding = parent.Window!.DipsToPixels(10);
        rtb.Margin = parent.Window!.DipsToPixels(10);
        rtb.Text = "Hello World";
    }
}
