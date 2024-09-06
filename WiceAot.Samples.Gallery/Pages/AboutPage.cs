namespace Wice.Samples.Gallery.Pages;

public partial class AboutPage : Page
{
    public AboutPage()
    {
        var stack = new Stack { Orientation = Orientation.Horizontal };
        SetDockType(stack, DockType.Top);
        Children.Add(stack);

        // load logo from embedded resource
        var logo = new Image
        {
            HorizontalAlignment = Alignment.Near,
            VerticalAlignment = Alignment.Near,
            Stretch = Stretch.None,
            Source = Application.Current.ResourceManager.GetWicBitmapSource(Assembly.GetExecutingAssembly(), typeof(Program).Namespace + ".Resources.aelyo_flat.png")!
        };
        stack.Children.Add(logo);

        // about & copyright
        var tb = new TextBox { VerticalAlignment = Alignment.Near, WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD };
        var asm = typeof(Application).Assembly;
        var url = "https://github.com/aelyo-softworks/Wice";
        tb.Text = "Wice AOT v" + asm.GetInformationalVersion() + " based on DirectN AOT v" + typeof(ComObject).Assembly.GetInformationalVersion() + Environment.NewLine
            + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription + " - " + DiagnosticsInformation.GetBitness() + Environment.NewLine
            + asm.GetCopyright() + Environment.NewLine
            + "Source code: " + url;
        stack.Children.Add(tb);

        tb.SetHyperLinkRange(url);

        // disclaimer
        var disc = new TextBox
        {
            WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD,
            Text = "THIS CODE AND INFORMATION IS PROVIDED ‘AS IS’ WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE." + Environment.NewLine
        };
        SetDockType(disc, DockType.Top);
        Children.Add(disc);

        var settings = new Stack { Orientation = Orientation.Horizontal };
        SetDockType(settings, DockType.Top);
        Children.Add(settings);

        var mipEnabled = Functions.IsMouseInPointerEnabled();
        var cb = new CheckBox { IsEnabled = !mipEnabled, Value = mipEnabled, VerticalAlignment = Alignment.Near };
        cb.Click += (s, e) =>
        {
            Functions.EnableMouseInPointer(new BOOL(true));
            cb.IsEnabled = false;
        };
        settings.Children.Add(cb);

        var txt = new TextBox
        {
            Padding = D2D_RECT_F.Thickness(10, 0),
            Text = "Is Mouse In Pointer Enabled",
            ToolTipContentCreator = (tt) => Window.CreateDefaultToolTipContent(tt,
                "Enables the mouse to act as a pointer input device and send WM_POINTER messages." + Environment.NewLine +
                "Can only be set once in the context of a process lifetime.")
        };
        settings.Children.Add(txt);

        var btn = new RoundedButton { Margin = D2D_RECT_F.Thickness(0, 10), VerticalAlignment = Alignment.Near, HorizontalAlignment = Alignment.Near };
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
            tlb.MaxButton!.IsVisible = false;
            tlb.MinButton!.IsVisible = false;
            dlg.Content.Children.Add(tlb);

            var pg = new PropertyGrid.PropertyGrid
            {
                CellMargin = D2D_RECT_F.Thickness(5, 0),
                //pg.MaxWidth = 600;
                //TextBox.WordWrappingProperty.SetValue(pg, DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_CHARACTER);
                Margin = D2D_RECT_F.Thickness(10),
                SelectedObject = new DiagnosticsInformation(null, Window)
            };
            dlg.Content.Children.Add(pg);
        };
        SetDockType(btn, DockType.Top);
        Children.Add(btn);
    }

    public override string HeaderText => base.HeaderText + " Wice";
    public override string ToolTipText => HeaderText;
    public override string IconText => MDL2GlyphResource.Info;
    public override int SortOrder => int.MaxValue;
    public override DockType DockType => DockType.Bottom;
}
