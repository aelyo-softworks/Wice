using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Windows.Dialog
{
    public class DialogSimpleSample : Sample
    {
        public override string Description => "A simple dialog. The Dialog type is a specific type of Popup.";

        public override void Layout(Visual parent)
        {
            var btn = new Button();
            btn.Text.Text = "Open a dialog...";
            btn.Click += (s, e) =>
            {
                var dlg = new Wice.Dialog();

                // a dialog is a children of a Window, it's not a standalone window
                parent.Window.Children.Add(dlg);
                dlg.Width = 300;
                dlg.Height = 300;
                dlg.RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.White.ToColor());

                var tb = new TextBox();
                tb.Text = "Press ESC to close";
                dlg.Content.Children.Add(tb);
            };

            btn.HorizontalAlignment = Alignment.Near; // remove from display
            Dock.SetDockType(btn, DockType.Top); // remove from display
            parent.Children.Add(btn);
        }
    }
}
