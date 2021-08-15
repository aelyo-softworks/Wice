using System.Collections.Generic;
using DirectN;
using Wice.Effects;
using Wice.Samples.Gallery.Pages;
using Wice.Samples.Gallery.Resources;

namespace Wice.Samples.Gallery
{
    public class GalleryWindow : Window
    {
        private Border _pageHolder;
        private readonly List<SymbolHeader> _headers = new List<SymbolHeader>();

        // define Window settings
        public GalleryWindow()
        {
            Title = I18n.T("appName");

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
            menuBack.Name = nameof(menuBack);
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
            var mainPage = new MainPage();
            mainPage.Name = nameof(mainPage);
            var mainHeader = AddPageHeader(mainPage, I18n.T("page.main"), MDL2GlyphResource.Home);
            mainHeader.SelectedButton.IsVisible = false;
            mainHeader.ToolTipContentCreator = tt => CreateDefaultToolTipContent(tt, I18n.T("page.main.tt"));
            Dock.SetDockType(mainHeader, DockType.Top);
            menu.Children.Add(mainHeader);

            var inputsPage = new InputsPage();
            var inputsHeader = AddPageHeader(inputsPage, I18n.T("page.inputs"), MDL2GlyphResource.Input);
            inputsHeader.SelectedButton.IsVisible = false;
            inputsHeader.ToolTipContentCreator = tt => CreateDefaultToolTipContent(tt, I18n.T("page.inputs.tt"));
            Dock.SetDockType(inputsHeader, DockType.Top);
            menu.Children.Add(inputsHeader);

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

        private SymbolHeader AddPageHeader(Page page, string text, string iconText)
        {
            var header = new SymbolHeader();
            header.Data = page;
            _headers.Add(header);
            header.Margin = D2D_RECT_F.Thickness(10, 0);
            header.Height = 40;
            header.Icon.Text = iconText;
            header.Text.Text = text;
            header.HoverRenderBrush = Compositor.CreateColorBrush(new _D3DCOLORVALUE(0x80C0C0C0));
            ConfigureHeaderText(header.Text);

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
            text.Margin = D2D_RECT_F.Thickness(10, 0, 10, 0);
            text.FontStretch = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_ULTRA_CONDENSED;
            text.DrawOptions = D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT;

            var typo = Typography.WithLigatures;
            text.SetTypography(typo.DWriteTypography.Object);
        }
    }
}
