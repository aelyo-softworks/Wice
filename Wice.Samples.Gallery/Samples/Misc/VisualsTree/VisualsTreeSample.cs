namespace Wice.Samples.Gallery.Samples.Misc.VisualsTree
{
    public class VisualsTreeSample : Sample
    {
        public override string Description => "Press the button (or F9 with debug builds) to open the VisualsTree window.";

        public override void Layout(Visual parent)
        {
            var btn = new Button();
            btn.Text.Text = "Open the VisualsTree Window";
            btn.Click += (s, e) =>
            {
                // note with .NET Framework you must reference Windows Forms to use the VisualsTree window
                var vt = new Wice.Utilities.VisualsTree();
                vt.SetCurrentWindow(parent.Window);
                vt.Show(NativeWindow.FromHandle(parent!.Window!.Handle));
            };

            btn.HorizontalAlignment = Alignment.Near; // remove from display
            Dock.SetDockType(btn, DockType.Top); // remove from display
            parent.Children.Add(btn);
        }
    }
}
