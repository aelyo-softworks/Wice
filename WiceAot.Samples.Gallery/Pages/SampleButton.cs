namespace Wice.Samples.Gallery.Pages;

// a visual button for a given sample
public partial class SampleButton : ButtonBase
{
    private readonly TextBox _icon = new();
    private readonly TextBox _title = new();
    private readonly TextBox _description = new();

    public SampleButton(SampleList sample)
    {
        ExceptionExtensions.ThrowIfNull(sample, nameof(sample));

        Child = new Dock();

        if (!string.IsNullOrEmpty(sample.IconText))
        {
            _icon.Text = sample.IconText;
            _icon.FontFamilyName = GetWindowTheme().SymbolFontName;
            _icon.VerticalAlignment = Alignment.Near;
            Dock.SetDockType(_icon, DockType.Left);
            Child.Children.Add(_icon);
        }

        _title.ForegroundBrush = new SolidColorBrush(GalleryWindow.ButtonColor);
        _title.Text = sample.Title;
        Dock.SetDockType(_title, DockType.Top);
        Child.Children.Add(_title);

        if (!string.IsNullOrEmpty(sample.Description))
        {
            _description.FontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_SEMI_LIGHT;
            _description.WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD;
            _description.Text = sample.SubTitle;
            _description.ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, _description.Text);
            Dock.SetDockType(_description, DockType.Top);
            Child.Children.Add(_description);
        }
    }

    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.LightGray.ChangeAlpha(128).ToColor());
        var theme = (GalleryTheme)GetWindowTheme();

        update();
        void update()
        {
            var margin = theme.SampleButtonMargin;
            Margin = margin;
            Width = theme.SampleButtonWidth;
            Height = theme.SampleButtonHeight;
            Child!.Margin = theme.SampleButtonChildMargin;

            _icon.Margin = margin;
            _description.Margin = margin;
            margin.top = 0;
            _title.Margin = margin;

            _icon.FontSize = theme.SampleButtonTextFontSize;
            _title.FontSize = theme.SampleButtonTextFontSize;
        }

        Window!.ThemeDpiChanged += (s, e) => update();
    }
}
