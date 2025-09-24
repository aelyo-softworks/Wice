namespace Wice.Samples.Gallery.Samples.Misc.TitleBar;

public class TitleBarSample : Sample
{
    public override string Description => "Opens a new window to see the Wice title bar visual.";

    public override void Layout(Visual parent)
    {
        var btn = new Button();
        btn.Click += (s, e) =>
        {
            var window = new Window
            {
                // we draw our own titlebar using Wice
                WindowsFrameMode = WindowsFrameMode.None,
                CreateOnCursorMonitor = true,
            };

            // add a Wice titlebar
            var titleBar = new Wice.TitleBar { IsMain = true };
            window.Children.Add(titleBar);

            window.ResizeClient(
                parent.Window!.DipsToPixels(400),
                parent.Window!.DipsToPixels(400)
            );
            window.Center();
            window.Show();
        };

        btn.Text.Text = "Open new window";
        btn.HorizontalAlignment = Alignment.Near; // remove from display
        Dock.SetDockType(btn, DockType.Top); // remove from display
        parent.Children.Add(btn);
    }
}
