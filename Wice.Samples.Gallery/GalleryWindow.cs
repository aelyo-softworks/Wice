using System.Collections.Generic;
using DirectN;
using Wice.Effects;
using Wice.Samples.Gallery.Pages;

namespace Wice.Samples.Gallery
{
    public class GalleryWindow : Window
    {
        public static _D3DCOLORVALUE ButtonColor;
        public static _D3DCOLORVALUE ButtonShadowColor;

        private const int _headersMargin = 10;
        private Border _pageHolder;
        private readonly List<SymbolHeader> _headers = new List<SymbolHeader>();

        static GalleryWindow()
        {
            ButtonColor = new _D3DCOLORVALUE(0xFF0078D7);
            ButtonShadowColor = ButtonColor.ChangeAlpha(0x7F);
        }

        // define Window settings
        public GalleryWindow()
        {
            // we draw our own titlebar using Wice itself
            WindowsFrameMode = WindowsFrameMode.None;

            // resize to 66% of the screen
            var monitor = Monitor.Primary.Bounds;
            ResizeClient(monitor.Width * 2 / 3, monitor.Height * 2 / 3);

            // the EnableBlurBehind call is necessary when using the Windows' acrylic
            // otherwise the window will be (almost) black
            Native.EnableBlurBehind();
            RenderBrush = AcrylicBrush.CreateAcrylicBrush(
                CompositionDevice,
                _D3DCOLORVALUE.White,
                0.2f,
                useWindowsAcrylic: true
                );

            AddControls();
        }

        // add basic controls for layout
        private void AddControls()
        {
            // add a Wice titlebar (looks similar to UWP)
            var titleBar = new TitleBar { IsMain = true };
            Children.Add(titleBar);

            var menuBack = new Border();
            menuBack.Width = 250;
            menuBack.RenderBrush = Compositor.CreateColorBrush(new _D3DCOLORVALUE(0xFFE6E6E6));
            menuBack.Opacity = 0.5f;
            SetLeft(menuBack, 0);
            Children.Add(menuBack);

            var gridPadding = 0;
            var grid = new Grid();
            grid.Columns[0].Size = menuBack.Width - gridPadding * 2;
            grid.Rows[0].Size = titleBar.Height;
            grid.Columns.Add(new GridColumn());
            grid.Rows.Add(new GridRow());
            Children.Add(grid);

            var document = new Border();
            _pageHolder = document;
            document.Margin = D2D_RECT_F.Thickness(20, 0);
            Grid.SetColumn(document, 1);
            Grid.SetRow(document, 1);
            grid.Children.Add(document);

            var menu = new Dock();
            menu.LastChildFill = false;
            Grid.SetRow(menu, 1);
            grid.Children.Add(menu);

            AddHeaderAndPages(menu);
        }

        // add headers & pages & selection logic
        private void AddHeaderAndPages(Dock menu)
        {
            // home page
            var homePage = new HomePage();
            var mainHeader = AddPageHeader(homePage);
            Dock.SetDockType(mainHeader, DockType.Top);
            menu.Children.Add(mainHeader);

            // all samples page
            var inputPage = new InputPage();
            var inputsHeader = AddPageHeader(inputPage);
            Dock.SetDockType(inputsHeader, DockType.Top);
            menu.Children.Add(inputsHeader);

            var layoutPage = new LayoutPage();
            var layoutHeader = AddPageHeader(layoutPage);
            Dock.SetDockType(layoutHeader, DockType.Top);
            menu.Children.Add(layoutHeader);

            // about page
            var aboutPage = new AboutPage();
            var aboutHeader = AddPageHeader(aboutPage);
            Dock.SetDockType(aboutHeader, DockType.Bottom);
            menu.Children.Add(aboutHeader);

            // select main
            mainHeader.IsSelected = true;
        }

        private void SelectPage(Page page)
        {
            // sets the current document to this page
            _pageHolder.Child = page;

            // deselect all other headers
            foreach (var header in _headers)
            {
                if (header.Data == page)
                    continue;

                header.IsSelected = false;
            }
        }

        private SymbolHeader AddPageHeader(Page page)
        {
            var header = new SymbolHeader();
            header.Data = page;
            _headers.Add(header);
            header.Margin = D2D_RECT_F.Thickness(_headersMargin, 0);
            header.Height = 40;
            header.Icon.Text = page.IconText;
            header.Text.Text = page.HeaderText;
            header.HoverRenderBrush = Compositor.CreateColorBrush(new _D3DCOLORVALUE(0x80C0C0C0));
            ConfigureHeaderText(header.Text);
            header.SelectedButton.IsVisible = false;
            header.ToolTipContentCreator = tt => CreateDefaultToolTipContent(tt, page.ToolTipText);

            header.IsSelectedChanged += (s, e) =>
            {
                if (e.Value)
                {
                    SelectPage(page);
                }
            };
            return header;
        }

        private void ConfigureHeaderText(TextBox text)
        {
            text.Margin = D2D_RECT_F.Thickness(_headersMargin, 0, _headersMargin, 0);
            text.FontStretch = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_ULTRA_CONDENSED;
            text.DrawOptions = D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT;

            var typo = Typography.WithLigatures;
            text.SetTypography(typo.DWriteTypography.Object);
        }
    }
}
