namespace Wice;

/// <summary>
/// A themed button composed of two <see cref="TextBox"/> children: an icon glyph and a text label,
/// hosted inside a panel (<see cref="Dock"/> by default).
/// </summary>
/// <remarks>
/// Behavior:
/// - Focusable and clickable (via <see cref="ButtonBase"/>).
/// - Sizes itself to its content by centering both horizontal and vertical alignments.
/// - Applies background styling from the current window theme when attached to composition.
/// - Dynamically updates spacing between icon and text on theme/DPI changes or when either text value changes.
/// </remarks>
/// <seealso cref="ButtonBase"/>
/// <seealso cref="TextBox"/>
/// <seealso cref="Dock"/>
public partial class Button : ButtonBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class.
    /// Creates the panel, icon, and text visuals; wires property change handlers; and configures default alignments.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="CreateIcon"/> or <see cref="CreateText"/> returns <see langword="null"/>.
    /// </exception>
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
    /// <remarks>
    /// Uses the window theme's <c>SymbolFontName</c>, is centered, color-font enabled, and is disabled to prevent user editing.
    /// </remarks>
    [Browsable(false)]
    public TextBox Icon { get; }

    /// <summary>
    /// Gets the text label of the button.
    /// </summary>
    /// <remarks>
    /// Centered, color-font enabled, and disabled to prevent user editing. Update its <see cref="TextBox.Text"/> to change the label.
    /// </remarks>
    [Browsable(false)]
    public TextBox Text { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the button's style (background brush) is updated from the window theme.
    /// </summary>
    /// <remarks>When true, <see cref="UpdateStyle"/> applies <c>Theme.ButtonColor</c> to <see cref="RenderVisual.RenderBrush"/>.</remarks>
    [Browsable(false)]
    public bool UpdateStyleFromTheme { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether margins are recomputed when the icon or text content changes.
    /// </summary>
    /// <remarks>When true, changing <see cref="TextBox.Text"/> on <see cref="Icon"/> or <see cref="Text"/> triggers <see cref="UpdateMargins"/>.</remarks>
    [Browsable(false)]
    public bool UpdateMarginsOnPropertyChanged { get; set; } = true;

    /// <summary>
    /// Subscribes to theme/DPI events and initializes style/margins once attached to composition.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event data.</param>
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
        Window!.ThemeDpiEvent += OnThemeDpiEvent;
    }

    /// <summary>
    /// Unsubscribes from theme/DPI events when detaching from composition.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event data.</param>
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
        UpdateMargins();
    }

    /// <summary>
    /// Reacts to icon text changes to recompute margins when enabled.
    /// </summary>
    /// <param name="sender">The icon text box.</param>
    /// <param name="e">Change information.</param>
    private void OnIconPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == TextBox.TextProperty.Name && UpdateMarginsOnPropertyChanged)
        {
            UpdateMargins();
        }
    }

    /// <summary>
    /// Reacts to label text changes to recompute margins when enabled.
    /// </summary>
    /// <param name="sender">The label text box.</param>
    /// <param name="e">Change information.</param>
    private void OnTextPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == TextBox.TextProperty.Name && UpdateMarginsOnPropertyChanged)
        {
            UpdateMargins();
        }
    }

    /// <summary>
    /// Updates the button's visual style. When <see cref="UpdateStyleFromTheme"/> is true,
    /// applies the theme's button color to the <see cref="RenderVisual.RenderBrush"/>.
    /// </summary>
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
    /// <remarks>
    /// Rules:
    /// - Both icon and text present: adds left spacing to the icon and left spacing to the text (with a slight vertical tweak for the icon).
    /// - Only text present: applies uniform theme margin to the text; clears icon margin.
    /// - Neither or only icon present: clears both margins so the content hugs the button edge.
    /// </remarks>
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
