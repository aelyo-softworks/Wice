using System.Collections.Generic;
using System.Linq;
using DirectN;
using Wice.Effects;
using Wice.Samples.Gallery.Pages;
using Wice.Utilities;

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
            //Native.EnableBlurBehind();
            RenderBrush = AcrylicBrush.CreateAcrylicBrush(
                CompositionDevice,
                _D3DCOLORVALUE.White,
                0.2f,
                useWindowsAcrylic: false
                );

            // uncomment this to enable Pointer messages
            //WindowsFunctions.EnableMouseInPointer();

            AddControls();
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

            // the document holds pages
            var document = new Border();
            _pageHolder = document;
            document.Margin = D2D_RECT_F.Thickness(20, 0, 0, 0);
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
            foreach (var page in Page.GetPages())
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
                // show page & handle exclusive select
                ShowPage(page);
                _headers.Cast<ISelectable>().Select(header);
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
