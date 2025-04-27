namespace WiceAot.Tests;

internal class Program
{
    [STAThread] // for webview2
    static void Main(string[] args)
    {
        if (Debugger.IsAttached)
        {
            using (var dw = new Application())
            {
                newWindow();
                dw.Run();
            }
        }
        else
        {
            try
            {
                using (var dw = new Application())
                {
                    newWindow();
                    dw.Run();
                }
            }
            catch (Exception e)
            {
                Application.AddError(e);
                Application.ShowFatalError(HWND.Null);
            }
        }

        void newWindow()
        {
            var win = new TestWindow { Title = "Wice" };
            //WindowsUtilities.AllocConsole();
            win.ResizeClient(1000, 800);
            win.Center();
            win.Show();
            win.SetForeground();
        }
    }
}
