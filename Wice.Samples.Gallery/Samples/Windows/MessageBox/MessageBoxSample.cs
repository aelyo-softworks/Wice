namespace Wice.Samples.Gallery.Samples.Windows.MessageBox
{
    public class MessageBoxSample : Sample
    {
        public override string Description => "A simple message box with one standard button. A MessageBox is a specific type of DialogBox.";

        public override void Layout(Visual parent)
        {
            var btn = new Button();
            btn.Text.Text = "Open a MessageBox...";
            btn.Click += (s, e) => Wice.MessageBox.Show(parent.Window, "Hello world!");

            btn.HorizontalAlignment = Alignment.Near; // remove from display
            Dock.SetDockType(btn, DockType.Top); // remove from display
            parent.Children.Add(btn);
        }
    }
}
