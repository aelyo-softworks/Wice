namespace Wice.Samples.Gallery.Pages;

// a visual button for a given sample
public partial class SampleButton : ButtonBase
{
    public SampleButton(SampleList sample)
    {
        ArgumentNullException.ThrowIfNull(sample);

        Margin = D2D_RECT_F.Thickness(5);
        DoWhenAttachedToComposition(() => RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.LightGray.ChangeAlpha(128).ToColor()));
        Width = 300;
        Height = 150;

        Child = new Dock
        {
            Margin = D2D_RECT_F.Thickness(10)
        };

        if (!string.IsNullOrEmpty(sample.IconText))
        {
            var icon = new TextBox
            {
                Margin = D2D_RECT_F.Thickness(5),
                Text = sample.IconText,
                FontSize = 20,
                FontFamilyName = Application.CurrentTheme.SymbolFontName,
                VerticalAlignment = Alignment.Near
            };
            Dock.SetDockType(icon, DockType.Left);
            Child.Children.Add(icon);
        }

        var title = new TextBox
        {
            Margin = D2D_RECT_F.Thickness(5, 0, 5, 5),
            ForegroundBrush = new SolidColorBrush(GalleryWindow.ButtonColor),
            Text = sample.Title,
            FontSize = 20
        };
        Dock.SetDockType(title, DockType.Top);
        Child.Children.Add(title);

        if (!string.IsNullOrEmpty(sample.Description))
        {
            var desc = new TextBox
            {
                Margin = D2D_RECT_F.Thickness(5),
                FontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_SEMI_LIGHT,
                WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD,
                Text = sample.SubTitle
            };
            desc.ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, desc.Text);
            Dock.SetDockType(desc, DockType.Top);
            Child.Children.Add(desc);
        }
    }
}
