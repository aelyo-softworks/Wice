namespace Wice.Samples.Gallery.Pages;

public partial class AboutPage : Page
{
#if NETFRAMEWORK
    private readonly PropertyGrid.PropertyGrid _pg = new();
#else
    private readonly PropertyGrid.PropertyGrid<SystemInformation> _pg = new();
#endif
    private readonly TextBox _mouseInPointerText = new();
    private readonly RoundedButton _systemInfoButton = new();
    private readonly CheckBox _mouseInPointerCheck = new();

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
            Source = Application.CurrentResourceManager.GetWicBitmapSource(Assembly.GetExecutingAssembly(), typeof(Program).Namespace + ".Resources.aelyo_flat.png")!
        };
        stack.Children.Add(logo);

        // about & copyright
        var tb = new TextBox { VerticalAlignment = Alignment.Near, WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD };
        var asm = typeof(Application).Assembly;
        var url = "https://github.com/aelyo-softworks/Wice";
#if NETFRAMEWORK
        tb.Text = "Wice for .NET Framework v" + asm.GetInformationalVersion() + " based on DirectN v" + typeof(ComObject).Assembly.GetInformationalVersion() + Environment.NewLine
#else
        tb.Text = "Wice for .NET Native AOT v" + asm.GetInformationalVersion() + " based on DirectN AOT v" + typeof(ComObject).Assembly.GetInformationalVersion() + Environment.NewLine
#endif
        + RuntimeInformation.FrameworkDescription + " - " + DiagnosticsInformation.GetBitness() + Environment.NewLine
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

        var mipEnabled = WiceCommons.IsMouseInPointerEnabled();
        _mouseInPointerCheck.IsEnabled = !mipEnabled;
        _mouseInPointerCheck.Value = mipEnabled;
        _mouseInPointerCheck.VerticalAlignment = Alignment.Center;
        _mouseInPointerCheck.Click += (s, e) =>
        {
            WiceCommons.EnableMouseInPointer(new BOOL(true));
            _mouseInPointerCheck.IsEnabled = false;
        };
        settings.Children.Add(_mouseInPointerCheck);

        _mouseInPointerText.Text = "Is Mouse In Pointer Enabled";
        _mouseInPointerText.ToolTipContentCreator = (tt) => Window.CreateDefaultToolTipContent(tt,
                "Enables the mouse to act as a pointer input device and send WM_POINTER messages." + Environment.NewLine +
                "Can only be set once in the context of a process lifetime.");
        settings.Children.Add(_mouseInPointerText);

        _systemInfoButton.VerticalAlignment = Alignment.Near;
        _systemInfoButton.HorizontalAlignment = Alignment.Near;
        _systemInfoButton.Text.Text = "System Info ...";
        var open = false;
        _systemInfoButton.Click += (s, e) =>
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

            _pg.MaxWidth = DesiredSize.width * 2 / 3;
            _pg.CellMargin = ((GalleryTheme)GetWindowTheme()).AboutPagePropertyGridCellMargin;
            _pg.Margin = ((GalleryTheme)GetWindowTheme()).AboutPagePropertyGridMargin;

#if NETFRAMEWORK
            _pg.SelectedObject = new DiagnosticsInformation(null, Window);
#else
            _pg.SelectedObject = new SystemInformation(null, Window);
#endif
            dlg.Content.Children.Add(_pg);
        };
        SetDockType(_systemInfoButton, DockType.Top);
        Children.Add(_systemInfoButton);
    }

    public override string HeaderText => base.HeaderText + " Wice";
    public override string ToolTipText => HeaderText;
    public override string IconText => MDL2GlyphResource.Info;
    public override int SortOrder => int.MaxValue;
    public override DockType DockType => DockType.Bottom;

    protected override void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        var theme = (GalleryTheme)GetWindowTheme();
        _systemInfoButton.Height = theme.AboutPageSystemInfoButtonHeight;
        _mouseInPointerText.Padding = theme.AboutPageMouseInPointerTextBoxPadding;
        _systemInfoButton.Margin = theme.AboutPageSystemInfoButtonMargin;

        _pg.CellMargin = theme.AboutPagePropertyGridCellMargin;
        _pg.Margin = theme.AboutPagePropertyGridMargin;

        if (_pg.SelectedObject != null)
        {
#if NETFRAMEWORK
            _pg.SelectedObject = new DiagnosticsInformation(null, Window);
#else
            _pg.SelectedObject = new SystemInformation(null, Window);
#endif
        }
    }
}
