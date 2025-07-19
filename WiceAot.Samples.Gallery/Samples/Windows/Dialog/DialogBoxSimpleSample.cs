namespace Wice.Samples.Gallery.Samples.Windows.Dialog;

public class DialogBoxSimpleSample : Sample
{
    public override string Description => "A simple dialog box. The DialogBox type is a specific type of Dialog.";
    public override int SortOrder => 1;

    public override void Layout(Visual parent)
    {
        var btn = new Button();
        btn.Text.Text = "Open a dialog box...";
        btn.Click += (s, e) =>
        {
            var dlg = new DialogBox();

            // a dialog box is a children of a Window, it's not a standalone window
            parent.Window!.Children.Add(dlg);
            dlg.Width = parent.Window!.DipsToPixels(300);
            dlg.Height = dlg.Width;
            dlg.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.White.ToColor());

            // add content to the dialog
            dlg.DialogContent.Children.Add(new Border { Width = parent.Window!.DipsToPixels(200), Height = parent.Window!.DipsToPixels(100) });

            // add standard (localized) buttons
            dlg.AddCancelButton();
            dlg.AddOkButton();
        };

        btn.HorizontalAlignment = Alignment.Near; // remove from display
        Dock.SetDockType(btn, DockType.Top); // remove from display
        parent.Children.Add(btn);
    }
}
