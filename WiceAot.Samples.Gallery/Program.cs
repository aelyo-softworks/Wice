namespace Wice.Samples.Gallery;

static class Program
{
    private static readonly string _storageDirectoryPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), typeof(Program).Namespace!);
    public static string StorageDirectoryPath => _storageDirectoryPath;

    //[STAThread] // wice doesn't require this
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
