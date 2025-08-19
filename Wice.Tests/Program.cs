namespace Wice.Tests;

class Program
{
    static void Main()
    {
        if (Application.IsDebuggerAttached)
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
                Application.ShowFatalError(IntPtr.Zero);
            }
        }

        static void newWindow()
        {
            var win = new TestWindow { Title = "Wice" };
            //WindowsUtilities.AllocConsole();
            win.ResizeClient(800, 600);
            win.Center();
            win.Show();
            win.SetForeground();
        }
    }
}
