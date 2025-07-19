namespace Wice.Samples.Gallery.Samples.Layout.Tabs;

public class TabsSample : Sample
{
    public override string Description => "A Tabs visual that simulate a browser's tabs.";

    public override void Layout(Visual parent)
    {
        var tabs = new Wice.Tabs();
        tabs.PagesHeader.Spacing = new D2D_SIZE_F(parent.Window!.DipsToPixels(5), parent.Window!.DipsToPixels(5));
        tabs.VerticalAlignment = Alignment.Near;
        Wice.Dock.SetDockType(tabs, DockType.Top); // remove from display
        parent.Children.Add(tabs);

        TabPage? plusPage = null;
        addPage();
        addPage();
        addPage();

        // add the special "+" tab, always at the end
        plusPage = new TabPage();
        tabs.Pages.Add(plusPage);
        plusPage.Header.AutoSelect = false; // it's not a selectable page per se
        plusPage.Header.Icon.Text = MDL2GlyphResource.Add; // plus icon
        plusPage.Header.Text.Text = string.Empty; // no text
        plusPage.Header.HorizontalAlignment = Alignment.Stretch; // take all available space
        plusPage.Header.HoverRenderBrush = Compositor!.CreateColorBrush(new D3DCOLORVALUE(0x80C0C0C0).ToColor());
        plusPage.Header.SelectedButtonClick += (s, e) => addPage();

        TabPage addPage()
        {
            var page = new TabPage();

            // take the plus page into account if it's there
            int index;
            if (plusPage != null)
            {
                index = plusPage.Index;
                tabs.Pages.Insert(index, page);
                page.Header.IsSelected = true;
            }
            else
            {
                index = tabs.Pages.Count;
                tabs.Pages.Add(page);
            }

            page.Header.Text.Text = "Page " + index;
            page.Header.SelectedBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.LightGray.ToColor());
            page.Header.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.DarkGray.ToColor());
            page.Header.HoverRenderBrush = Compositor.CreateColorBrush(new D3DCOLORVALUE(0x80C0C0C0).ToColor());

            // by default header's close button is invisible
            page.Header.CloseButton!.IsVisible = true;
            page.Header.CloseButtonClick += (s, e) => tabs.Pages.Remove(page);

            page.Content = new TextBox
            {
                Margin = D2D_RECT_F.Thickness(parent.Window!.DipsToPixels(10), parent.Window!.DipsToPixels(50), parent.Window!.DipsToPixels(10), parent.Window!.DipsToPixels(50)),
                Text = "This is the content of page #" + index,
            };

            return page;
        }
    }
}
