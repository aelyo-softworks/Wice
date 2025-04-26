using System;
using System.Diagnostics;
using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery
{
    static class Program
    {
        private static readonly string _storageDirectoryPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), typeof(Program).Namespace);
        public static string StorageDirectoryPath => _storageDirectoryPath;

        [STAThread] // Wice doesn't require this but it's needed for Drag & Drop operations
        static void Main()
        {
            Application.AllApplicationsExit += (s, e) =>
            {
#if DEBUG
                ComObjectLogger.Instance.Dispose();
                UILogger.Instance.Dispose();
                DirectNLogger.Instance.Dispose();
#else
                Audit.StopAuditing();
#endif
            };
#if DEBUG
            Application.Logger = UILogger.Instance;
            MFMediaTypeWrapper.Logger = DirectNLogger.Instance;
#else
            Audit.StartAuditing(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), typeof(Program).Namespace, "logs"));
            Audit.FlushRecords();
#endif

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

            void newWindow()
            {
                using (var dw = new Application())
                {
                    using (var win = new GalleryWindow())
                    {
                        win.Center();
                        win.Show();
                        dw.Run();
                    }
                }
            }
        }
    }
}
