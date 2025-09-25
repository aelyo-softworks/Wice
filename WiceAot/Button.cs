namespace Wice;

/// <summary>
/// A themed button composed of two <see cref="TextBox"/> children: an icon glyph and a text label,
/// hosted inside a panel (<see cref="Dock"/> by default).
/// </summary>
public partial class Button : ButtonBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class.
    /// Creates the panel, icon, and text visuals; wires property change handlers; and configures default alignments.
    /// </summary>
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

    /// <summary>
    /// Gets the icon text box used to render a symbol/glyph.
    /// </summary>
    [Browsable(false)]
    public TextBox Icon { get; }

    /// <summary>
    /// Gets the text label of the button.
    /// </summary>
    [Browsable(false)]
    public TextBox Text { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the button's style (background brush) is updated from the window theme.
    /// </summary>
    [Browsable(false)]
    public bool UpdateStyleFromTheme { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether margins are recomputed when the icon or text content changes.
    /// </summary>
    [Browsable(false)]
    public bool UpdateMargins { get; set; } = true;

    /// <inheritdoc/>
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
        Window!.ThemeDpiEvent += OnThemeDpiEvent;
    }

    /// <inheritdoc/>
    protected override void OnDetachingFromComposition(object? sender, EventArgs e)
    {
        base.OnDetachingFromComposition(sender, e);
        Window!.ThemeDpiEvent -= OnThemeDpiEvent;
    }

    /// <summary>
    /// Handles theme/DPI updates by refreshing visual style and spacing.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Theme/DPI event data.</param>
    protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        UpdateStyle();
        if (UpdateMargins)
        {
            DoUpdateMargins();
        }
    }

    private void OnIconPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == TextBox.TextProperty.Name && UpdateMargins)
        {
            DoUpdateMargins();
        }
    }

    private void OnTextPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == TextBox.TextProperty.Name && UpdateMargins)
        {
            DoUpdateMargins();
        }
    }

    /// <inheritdoc/>
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

    /// <summary>
    /// Computes and applies margins for <see cref="Icon"/> and <see cref="Text"/> based on their content presence.
    /// </summary>
    protected virtual void DoUpdateMargins()
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

    /// <summary>
    /// Creates the panel hosting the icon and text visuals.
    /// </summary>
    /// <returns>A new <see cref="Dock"/> instance.</returns>
    protected virtual Visual CreatePanel() => new Dock();

    /// <summary>
    /// Creates the icon <see cref="TextBox"/> configured for symbol fonts.
    /// </summary>
    /// <returns>A disabled, center-aligned color-font-capable <see cref="TextBox"/> using the theme's symbol font.</returns>
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

    /// <summary>
    /// Creates the label <see cref="TextBox"/>.
    /// </summary>
    /// <returns>A disabled, center-aligned color-font-capable <see cref="TextBox"/>.</returns>
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
