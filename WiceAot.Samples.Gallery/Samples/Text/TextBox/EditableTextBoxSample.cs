namespace Wice.Samples.Gallery.Samples.Text.TextBox;

public class EditableTextBoxSample : Sample
{
    public override string Description => "A simple editable text box.";
    public override int SortOrder => 1;

    public override void Layout(Visual parent)
    {
        var tb = new Wice.TextBox { IsFocusable = true };
        parent.Children.Add(tb);
        Dock.SetDockType(tb, DockType.Top);
        tb.IsEditable = true;

        tb.SelectionBrush = new SolidColorBrush(D3DCOLORVALUE.Green);
        tb.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.LightSalmon.ToColor());
        tb.Padding = parent.Window!.DipsToPixels(10);
        tb.Margin = parent.Window!.DipsToPixels(10);
        tb.Text = "Click me and edit!";
    }
}
