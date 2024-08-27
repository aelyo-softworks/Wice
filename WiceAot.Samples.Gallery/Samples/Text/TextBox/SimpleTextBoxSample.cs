namespace Wice.Samples.Gallery.Samples.Text.TextBox;

public class SimpleTextBoxSample : Sample
{
    public override string Description => "A simple non editable text box.";

    public override void Layout(Visual parent)
    {
        var tb = new Wice.TextBox();
        parent.Children.Add(tb);
        Dock.SetDockType(tb, DockType.Top);

        tb.SelectionBrush = new SolidColorBrush(D3DCOLORVALUE.Red);
        tb.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.GreenYellow.ToColor());
        tb.Padding = D2D_RECT_F.Thickness(10);
        tb.Margin = D2D_RECT_F.Thickness(10);
        tb.Text = "Hello World";
    }
}
