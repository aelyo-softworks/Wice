using System;
using System.Linq;
using System.Reflection;
using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Pages
{
    public class AboutPage : Page
    {
        public AboutPage()
        {
            var stack = new Stack();
            stack.Orientation = Orientation.Horizontal;
            SetDockType(stack, DockType.Top);
            Children.Add(stack);

            // load logo from embedded resource
            var logo = new Image();
            logo.HorizontalAlignment = Alignment.Near;
            logo.VerticalAlignment = Alignment.Near;
            logo.Stretch = Stretch.None;
            logo.Source = Application.Current.ResourceManager.GetWicBitmapSource(Assembly.GetExecutingAssembly(), typeof(Program).Namespace + ".Resources.aelyo_flat.png");
            stack.Children.Add(logo);

            // about & copyright
            var tb = new TextBox();
            tb.VerticalAlignment = Alignment.Near;
            tb.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
            var asm = typeof(Application).Assembly;
            var url = "https://github.com/aelyo-softworks/Wice";
            tb.Text = "Wice v" + asm.GetInformationalVersion() + Environment.NewLine
                + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription + Environment.NewLine
                + asm.GetCopyright() + Environment.NewLine
                + "Source code: " + url;
            stack.Children.Add(tb);

            tb.SetHyperLinkRange(url);

            // disclaimer
            var disc = new TextBox();
            disc.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
            disc.Text = "THIS CODE AND INFORMATION IS PROVIDED ‘AS IS’ WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.";
            Children.Add(disc);
        }

        public override string HeaderText => base.HeaderText + " Wice";
        public override string ToolTipText => HeaderText;
        public override string IconText => MDL2GlyphResource.Info;
    }
}
