namespace Wice.Samples.Gallery;

static class Program
{
    [STAThread] // Wice doesn't require this but it's needed for Drag&Drop operations
    static void Main()
    {
        if (Debugger.IsAttached)
        {
            newWindow();
        }
        else
        {
            try
            {
                newWindow();
            }
            catch (Exception e)
            {
                Application.AddError(e);
                Application.ShowFatalError(IntPtr.Zero);
            }
        }

        static void newWindow()
        {
            using var dw = new Application();
            using var win = new GalleryWindow();
            win.Center();
            win.Show();
            dw.Run();
        }
    }
}
