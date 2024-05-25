using System.Diagnostics;
using DirectN;
using Wice;

namespace WiceAot.Tests
{
    internal class Program
    {
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
                win.ResizeClient(400, 900);
                win.Center();
                win.Show();
                win.SetForeground();
            }
        }
    }
}
