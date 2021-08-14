using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectN;
using Wice.Effects;

namespace Wice.Samples.Gallery
{
    public class GalleryWindow : Window
    {
        public GalleryWindow()
        {
            // we draw our own titlebar using Wice itself
            WindowsFrameMode = WindowsFrameMode.None;

            // resize to 66% of the screen
            var monitor = Monitor.Primary.Bounds;
            ResizeClient(monitor.Width * 2 / 3, monitor.Height * 2 / 3);

            // add a Wice titlebar (looks similar to UWP)
            Children.Add(new TitleBar { IsMain = true });

            // the EnableBlurBehind call is necessary when using the Windows' acrylic
            // otherwise the window will be (almost) black
            Native.EnableBlurBehind();
            RenderBrush = AcrylicBrush.CreateAcrylicBrush(
                CompositionDevice,
                _D3DCOLORVALUE.White,
                0.2f,
                useWindowsAcrylic: true
                );
        }
    }
}
