namespace Wice;

public partial class Button : ButtonBase
{
    public Button()
    {
        IsFocusable = true;
        Child = CreatePanel();
        Cursor = Cursor.Hand;

        Icon = CreateIcon();
        if (Icon == null)
            throw new InvalidOperationException();

        Icon.PropertyChanged += OnIconPropertyChanged;
#if DEBUG
        Icon.Name ??= nameof(Icon);
#endif
        Child.Children.Add(Icon);

        // not stretch, for text
        Text = CreateText();
        if (Text == null)
            throw new InvalidOperationException();

        Text.PropertyChanged += OnTextPropertyChanged;
#if DEBUG
        Text.Name ??= nameof(Text);
#endif
        Child.Children.Add(Text);

        // to ensure button size is equal to content's size
        HorizontalAlignment = Alignment.Center;
        VerticalAlignment = Alignment.Center;
    }

    [Browsable(false)]
    public TextBox Icon { get; }

    [Browsable(false)]
    public TextBox Text { get; }

    [Browsable(false)]
    public bool UpdateStyleFromTheme { get; set; } = true;

    [Browsable(false)]
    public bool UpdateMarginsOnPropertyChanged { get; set; } = true;

    private void OnIconPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == TextBox.TextProperty.Name && UpdateMarginsOnPropertyChanged)
        {
            UpdateMargins();
        }
    }

    private void OnTextPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == TextBox.TextProperty.Name && UpdateMarginsOnPropertyChanged)
        {
            UpdateMargins();
        }
    }

    protected override void UpdateStyle()
    {
        base.UpdateStyle();
        if (UpdateStyleFromTheme)
        {
            var compositor = Compositor;
            if (compositor != null)
            {
                RenderBrush = compositor.CreateColorBrush(GetWindowTheme().ButtonColor.ToColor());
            }
        }
    }

    protected virtual void UpdateMargins()
    {
        if (!string.IsNullOrEmpty(Icon.Text) && !string.IsNullOrEmpty(Text.Text))
        {
            Text.Margin = D2D_RECT_F.Thickness(0, GetWindowTheme().ButtonMargin, GetWindowTheme().ButtonMargin, GetWindowTheme().ButtonMargin);
            Icon.Margin = D2D_RECT_F.Thickness(GetWindowTheme().ButtonMargin, GetWindowTheme().ButtonMargin - 2, GetWindowTheme().ButtonMargin, GetWindowTheme().ButtonMargin);
        }
        else if (!string.IsNullOrEmpty(Text.Text))
        {
            Text.Margin = D2D_RECT_F.Thickness(GetWindowTheme().ButtonMargin, GetWindowTheme().ButtonMargin, GetWindowTheme().ButtonMargin, GetWindowTheme().ButtonMargin);
            Icon.Margin = new D2D_RECT_F();
        }
        else
        {
            Text.Margin = new D2D_RECT_F();
            Icon.Margin = new D2D_RECT_F();
        }
    }

    protected virtual Visual CreatePanel() => new Dock();
    protected virtual TextBox CreateIcon()
    {
        var tb = new TextBox
        {
            FontFamilyName = GetWindowTheme().SymbolFontName,
            ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER,
            DrawOptions = D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT,
            IsEnabled = false
        };
        return tb;
    }

    protected virtual TextBox CreateText()
    {
        var tb = new TextBox
        {
            ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER,
            DrawOptions = D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT,
            IsEnabled = false
        };
        return tb;
    }
}
