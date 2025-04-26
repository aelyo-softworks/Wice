using System;
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
            logo.Source = Application.CurrentResourceManager.GetWicBitmapSource(Assembly.GetExecutingAssembly(), typeof(Program).Namespace + ".Resources.aelyo_flat.png");
            stack.Children.Add(logo);

            // about & copyright
            var tb = new TextBox();
            tb.VerticalAlignment = Alignment.Near;
            tb.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
            var asm = typeof(Application).Assembly;
            var url = "https://github.com/aelyo-softworks/Wice";
            tb.Text = "Wice v" + asm.GetInformationalVersion() + " based on DirectN v" + typeof(ComObject).Assembly.GetInformationalVersion() + Environment.NewLine
                + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription + " - " + DiagnosticsInformation.GetBitness() + Environment.NewLine
                + asm.GetCopyright() + Environment.NewLine
                + "Source code: " + url;
            stack.Children.Add(tb);

            tb.SetHyperLinkRange(url);

            // disclaimer
            var disc = new TextBox();
            disc.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
            disc.Text = "THIS CODE AND INFORMATION IS PROVIDED ‘AS IS’ WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE." + Environment.NewLine;
            SetDockType(disc, DockType.Top);
            Children.Add(disc);

            var settings = new Stack();
            settings.Orientation = Orientation.Horizontal;
            SetDockType(settings, DockType.Top);
            Children.Add(settings);

            var mipEnabled = WindowsFunctions.IsMouseInPointerEnabled();
            var cb = new CheckBox();
            cb.IsEnabled = !mipEnabled;
            cb.Value = mipEnabled;
            cb.VerticalAlignment = Alignment.Near;
            cb.Click += (s, e) =>
            {
                WindowsFunctions.EnableMouseInPointer();
                cb.IsEnabled = false;
            };
            settings.Children.Add(cb);

            var txt = new TextBox();
            txt.Padding = D2D_RECT_F.Thickness(10, 0);
            txt.Text = "Is Mouse In Pointer Enabled";
            txt.ToolTipContentCreator = (tt) => Window.CreateDefaultToolTipContent(tt,
                "Enables the mouse to act as a pointer input device and send WM_POINTER messages." + Environment.NewLine +
                "Can only be set once in the context of a process lifetime.");
            settings.Children.Add(txt);

            var btn = new RoundedButton();
            btn.Margin = D2D_RECT_F.Thickness(0, 10);
            btn.VerticalAlignment = Alignment.Near;
            btn.HorizontalAlignment = Alignment.Near;
            btn.Text.Text = "System Info ...";
            btn.Height = 30;
            var open = false;
            btn.Click += (s, e) =>
            {
                if (open)
                    return;

                open = true;
                var dlg = new DialogBox();
                dlg.Closed += (s2, e2) => open = false;
                Children.Add(dlg);

                var tlb = new TitleBar();
                tlb.MaxButton.IsVisible = false;
                tlb.MinButton.IsVisible = false;
                dlg.Content.Children.Add(tlb);

                var pg = new PropertyGrid.PropertyGrid();
                pg.CellMargin = D2D_RECT_F.Thickness(5, 0);
                //pg.MaxWidth = 600;
                //TextBox.WordWrappingProperty.SetValue(pg, DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_CHARACTER);
                pg.Margin = D2D_RECT_F.Thickness(10);
                pg.SelectedObject = new DiagnosticsInformation(null, Window);
                dlg.Content.Children.Add(pg);
            };
            SetDockType(btn, DockType.Top);
            Children.Add(btn);
        }

        private void Cb_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public override string HeaderText => base.HeaderText + " Wice";
        public override string ToolTipText => HeaderText;
        public override string IconText => MDL2GlyphResource.Info;
        public override int SortOrder => int.MaxValue;
        public override DockType DockType => DockType.Bottom;
    }
}
