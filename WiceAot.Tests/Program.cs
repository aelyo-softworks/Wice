namespace WiceAot.Tests;

internal class Program
{
    [STAThread] // for webview2
    static void Main()
    {
        WindowSynchronizationContext.WithContext(() => // ensure we have a UI-thread bound synchronization context for async calls
        {
            if (Debugger.IsAttached)
            {
                using var dw = new Application();
                newWindow();
                dw.Run();
            }
            else
            {
                try
                {
                    using var dw = new Application();
                    newWindow();
                    dw.Run();
                }
                catch (Exception e)
                {
                    Application.AddError(e);
                    Application.ShowFatalError(HWND.Null);
                }
            }

            static void newWindow()
            {
                var win = new TestWindow { Title = "Wice AOT" };
                //WindowsUtilities.AllocConsole();
                win.ResizeClient(1000, 800);
                win.Center();
                win.Show();
                win.SetForeground();
            }
        });
    }
}
