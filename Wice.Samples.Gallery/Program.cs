using System;
using System.Diagnostics;
using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            Application.Logger = UILogger.Instance;
            MFMediaTypeWrapper.Logger = DirectNLogger.Instance;
#else
            Audit.StartAuditing(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), typeof(Program).Namespace, "logs"));
            Audit.FlushRecords();
#endif

            try
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
            }
            finally
            {
#if DEBUG
                ComObjectLogger.Instance.Dispose();
                UILogger.Instance.Dispose();
                DirectNLogger.Instance.Dispose();
#else
                Audit.StopAuditing();
#endif
            }

            void newWindow()
            {
                using (var dw = new Application())
                {
                    var win = new GalleryWindow();
                    //win.ResizeClient(400, 900);
                    win.Center();
                    win.Show();
                    dw.Run();
                }
            }
        }
    }
}
