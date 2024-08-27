namespace Wice.Samples.Gallery.Samples.Input.Button;

public class ButtonDockSample : Sample
{
    public override int SortOrder => 1;
    public override string Description => "Two buttons in a Dock";

    public override void Layout(Visual parent)
    {
        var dock = new Dock { Height = 50, LastChildFill = false, RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Olive.ToColor()) };
        parent.Children.Add(dock);
        Dock.SetDockType(dock, DockType.Top); // remove from display

        var okButton = new Wice.Button { Margin = 10, MinWidth = 70 };
        okButton.Click += (s, e) => MessageBox.Show(parent.Window!, "You pressed OK");
        okButton.Text.Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER;
        okButton.Text.Text = "OK";
        Dock.SetDockType(okButton, DockType.Right);
        dock.Children.Add(okButton);

        var cancelButton = new Wice.Button { Margin = 3, MinWidth = 70 };
        cancelButton.Click += (s, e) => MessageBox.Show(parent.Window!, "You pressed Cancel");
        cancelButton.Text.Text = "Cancel";
        cancelButton.Icon.Text = MDL2GlyphResource.Cancel;
        Dock.SetDockType(cancelButton, DockType.Right);
        dock.Children.Add(cancelButton);
    }
}
