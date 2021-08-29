namespace Wice.Samples.Gallery.Samples.Misc.TitleBar
{
    public class TitleBarSample : Sample
    {
        public override string Description => "Opens a new window to see the Wice title bar visual.";

        public override void Layout(Visual parent)
        {
            var btn = new Button();
            btn.Click += (s, e) =>
            {
                var window = new Window();

                // we draw our own titlebar using Wice
                window.WindowsFrameMode = WindowsFrameMode.None;
                window.Center();
                window.Show();

                // add a Wice titlebar
                var titleBar = new Wice.TitleBar { IsMain = true };
                window.Children.Add(titleBar);
                // remove from display - this is here because there's a bug where the title bar is not refreshed
                window.ResizeClient(400, 400);
            };

            btn.Text.Text = "Open new window";
            btn.HorizontalAlignment = Alignment.Near; // remove from display
            Dock.SetDockType(btn, DockType.Top); // remove from display
            parent.Children.Add(btn);
        }
    }
}
