namespace Wice.Samples.Gallery.Samples.Misc.Application;

public class MultipleApplicationSample : Sample
{
    public override string Description => "Any number of applications can be created, each associated to a UI thread, each owning its own set of windows.";

    public override void Layout(Visual parent)
    {
        var btn = new Button();
        btn.Text.Text = "Open new Window in a new Application";
        btn.HorizontalAlignment = Alignment.Near; // remove from display
        Dock.SetDockType(btn, DockType.Top); // remove from display
        btn.Click += (s, e) =>
        {
            TaskUtilities.RunWithNewSTAThread(() =>
            {
                // create another application in the same process
                using var dw = new Wice.Application();
                // add (main) window to application
                // this window is not background (by default) so it will outlive the main gallery application window
                // and will need to be closed separately to end the process
                var win = new Window
                {
                    Title = "Window in another application (Thread " + Environment.CurrentManagedThreadId + ")",
                    WindowsFrameMode = WindowsFrameMode.None,
                    CreateOnCursorMonitor = true
                };
                win.Children.Add(new Wice.TitleBar { IsMain = true });
                win.ResizeClient(parent.Window!.DipsToPixels(400), parent.Window!.DipsToPixels(400));
                win.Center();
                win.Show();
                dw.Run();

                // put this here to ensure .NET will not stay blocked on the thread...
                // if you put it before creating the app, the app will be killed if the main thread exits
                Thread.CurrentThread.IsBackground = true;
            });
        };
        parent.Children.Add(btn);
    }
}
