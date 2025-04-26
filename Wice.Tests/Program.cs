using System;
using System.Diagnostics;
using DirectN;
using Wice.Utilities;

namespace Wice.Tests
{
    class Program
    {
        static void Main()
        {
#if DEBUG
            //ComObject.Logger = ComObjectLogger.Instance;
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
                var win = new TestWindow { Title = "Wice" };
                //WindowsUtilities.AllocConsole();
                win.ResizeClient(800, 600);
                win.Center();
                win.Show();
                win.SetForeground();
            }
        }
    }
}
