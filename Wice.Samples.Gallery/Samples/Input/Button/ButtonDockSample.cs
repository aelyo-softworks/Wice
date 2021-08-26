using DirectN;

namespace Wice.Samples.Gallery.Samples.Input.Button
{
    public class ButtonDockSample : Sample
    {
        public override int SortOrder => 1;
        public override string Description => "Two buttons in a Dock";

        public override void Layout(Visual parent)
        {
            var dock = new Dock();
            dock.Height = 50;
            dock.LastChildFill = false;
            dock.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Olive);
            parent.Children.Add(dock);
            Wice.Dock.SetDockType(dock, DockType.Top); // remove from display

            var okButton = new Wice.Button();
            okButton.Margin = 10;
            okButton.MinWidth = 70;
            okButton.Click += (s, e) => MessageBox.Show(parent.Window, "You pressed OK");
            okButton.Text.Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER;
            okButton.Text.Text = "OK";
            Dock.SetDockType(okButton, DockType.Right);
            dock.Children.Add(okButton);

            var cancelButton = new Wice.Button();
            cancelButton.Margin = 3;
            cancelButton.MinWidth = 70;
            cancelButton.Click += (s, e) => MessageBox.Show(parent.Window, "You pressed Cancel");
            cancelButton.Text.Text = "Cancel";
            cancelButton.Icon.Text = MDL2GlyphResource.Cancel;
            Dock.SetDockType(cancelButton, DockType.Right);
            dock.Children.Add(cancelButton);
        }
    }
}
