namespace Wice.Samples.Gallery;

public sealed partial class GalleryWindow : Window, IDisposable
{
    public static D3DCOLORVALUE ButtonColor { get; } = new D3DCOLORVALUE(0xFF0078D7);
    public static D3DCOLORVALUE ButtonShadowColor { get; } = ButtonColor.ChangeAlpha(0x7F);

    private const int _headersMargin = 10;
    private Border? _pageHolder;
    private readonly List<SymbolHeader> _headers = [];
    private readonly List<Page> _pages = [];

    // define Window settings
    public GalleryWindow()
    {
        // we draw our own titlebar using Wice itself
        WindowsFrameMode = WindowsFrameMode.None;

        // resize to 66% of the screen
        var monitor = GetMonitor();
        if (monitor != null)
        {
            var bounds = monitor.Bounds;
            ResizeClient(bounds.Width * 2 / 3, bounds.Height * 2 / 3);
        }

        var windows11 = WindowsVersionUtilities.KernelVersion > new Version(10, 0, 22000);

        // the EnableBlurBehind call may be necessary when using the Windows' acrylic depending on Windows version
        // otherwise the window will be (almost) black
        EnableBlurBehind();
        RenderBrush = AcrylicBrush.CreateAcrylicBrush(
            CompositionDevice,
            D3DCOLORVALUE.White,
            0.2f,
            useWindowsAcrylic: windows11
            );

        // uncomment this to enable Pointer messages at startup time
        //WindowsFunctions.EnableMouseInPointer();

        AddControls();
    }

    // create on the monitor where the mouse is at startup
    // remove this if you always want to create on primary monitor
    public override RECT CreateRect
    {
        get
        {
            var cursor = NativeWindow.GetCursorPosition();
            var area = Monitor.All.First(m => m.Handle == Monitor.GetNearestFromPoint(cursor.x, cursor.y)).WorkingArea;
            return area;
        }
    }

    public void ShowPage(Visual page)
    {
        if (_pageHolder == null)
            return; // too early

        _pageHolder.Child = page;
    }

    // add basic controls for layout
    private void AddControls()
    {
        // add a Wice titlebar (looks similar to UWP)
        var titleBar = new TitleBar { IsMain = true };

        Children.Add(titleBar);

        var menuBack = new Border
        {
            Width = 250,
            RenderBrush = Compositor!.CreateColorBrush(new D3DCOLORVALUE(0xFFE6E6E6).ToColor()),
            Opacity = 0.5f
        };
        SetLeft(menuBack, 0);
        Children.Add(menuBack);

        var gridPadding = 0;
        var grid = new Grid();
        grid.Columns[0].Size = menuBack.Width - gridPadding * 2;
        grid.Rows[0].Size = titleBar.Height;
        grid.Columns.Add(new GridColumn());
        grid.Rows.Add(new GridRow());
        Children.Add(grid);

        // this code is used to handle DPI changes
        titleBar.Updated += (s, e) =>
        {
            grid.Rows[0].Size = titleBar.Height;
        };

        // the document holds pages
        var document = new Border();
        _pageHolder = document;
        document.Margin = D2D_RECT_F.Thickness(20, 0, 0, 0);
        Grid.SetColumn(document, 1);
        Grid.SetRow(document, 1);
        grid.Children.Add(document);

        var menu = new Dock
        {
            LastChildFill = false
        };
        Grid.SetRow(menu, 1);
        grid.Children.Add(menu);

        AddHeaderAndPages(menu);
    }

    // add headers & pages & selection logic
    private void AddHeaderAndPages(Dock menu)
    {
        _pages.AddRange(Page.GetPages());
        foreach (var page in _pages)
        {
            var header = AddPageHeader(page);
            Dock.SetDockType(header, page.DockType);
            menu.Children.Add(header);

            // select main
            if (page is HomePage)
            {
                header.IsSelected = true;
            }
        }
    }

    private SymbolHeader AddPageHeader(Page page)
    {
        var header = new SymbolHeader { Data = page };
        _headers.Add(header);
        header.Margin = D2D_RECT_F.Thickness(_headersMargin, 0);
        header.Height = 40;
        header.Icon.Text = page.IconText;
        header.Text.Text = page.HeaderText;
        header.Text.IsEnabled = false;
        header.AccessKeys.Add(new AccessKey(VIRTUAL_KEY.VK_SPACE));
        header.HoverRenderBrush = Compositor!.CreateColorBrush(new D3DCOLORVALUE(0x80C0C0C0).ToColor());
        ConfigureHeaderText(header.Text);
        header.SelectedButton.IsVisible = false;
        header.ToolTipContentCreator = tt => CreateDefaultToolTipContent(tt, page.ToolTipText);

        header.IsSelectedChanged += (s, e) =>
        {
            // show page & handle exclusive select
            ShowPage(page);
            _headers.Cast<ISelectable>().Select(header);
        };
        return header;
    }

    private static void ConfigureHeaderText(TextBox text)
    {
        text.Margin = D2D_RECT_F.Thickness(_headersMargin, 0, _headersMargin, 0);
        text.FontStretch = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_ULTRA_CONDENSED;
        text.DrawOptions = D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT;

        var typo = Typography.WithLigatures;
        text.SetTypography(typo.DWriteTypography?.Object);
    }

    public void Dispose() => _pages.Dispose();
}
