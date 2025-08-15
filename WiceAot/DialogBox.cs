﻿namespace Wice;

/// <summary>
/// A themed, modal dialog box composed of an optional rounded border, a vertical back panel,
/// an optional <see cref="TitleBar"/>, a content area, and an optional buttons panel.
/// </summary>
/// <remarks>
/// Behavior:
/// - Initializes child visuals (border, back panel, title bar, dialog content, buttons panel) in the expected order.
/// - Provides helpers to add command buttons (OK/Cancel/Yes/No/Close) with access keys and default result wiring.
/// - Updates layout metrics (margins, font sizes, corner radius) on theme/DPI changes.
/// - Syncs its own <see cref="Visual.Width"/> and <see cref="Visual.Height"/> to the arranged size of the back panel.
/// </remarks>
/// <seealso cref="Dialog"/>
/// <seealso cref="TitleBar"/>
/// <seealso cref="Button"/>
public partial class DialogBox : Dialog
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DialogBox"/> class.
    /// </summary>
    /// <remarks>
    /// - Creates and inserts an optional <see cref="Border"/> (rounded rectangle).
    /// - Creates the <see cref="BackPanel"/> (required) and adds it to the dialog content.
    /// - Creates the optional <see cref="TitleBar"/> and adds it to <see cref="BackPanel"/> when present.
    /// - Creates the <see cref="DialogContent"/> (required) and adds it to <see cref="BackPanel"/>.
    /// - Creates the optional <see cref="ButtonsPanel"/> and adds it to <see cref="BackPanel"/> when present.
    /// - In DEBUG builds, assigns diagnostic names to created visuals when not already set.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="CreateBackPanel"/> or <see cref="CreateDialogContent"/> returns null.
    /// </exception>
    public DialogBox()
    {
        Border = CreateBorder();
        if (Border != null)
        {
            Content.Children.Add(Border);
        }

        BackPanel = CreateBackPanel();
        if (BackPanel == null)
            throw new InvalidOperationException();

#if DEBUG
        BackPanel.Name ??= "backPanel";
#endif
        Content.Children.Add(BackPanel);

        TitleBar = CreateTitleBar();
#if DEBUG
        if (TitleBar != null)
        {
            TitleBar.Name ??= "titleBar";
        }
#endif

        if (TitleBar != null)
        {
            BackPanel.Children.Add(TitleBar);
        }

        DialogContent = CreateDialogContent();
        if (DialogContent == null)
            throw new InvalidOperationException();
#if DEBUG
        DialogContent.Name ??= "dialogContent";
#endif
        BackPanel.Children.Add(DialogContent);

        ButtonsPanel = CreateButtonsPanel();
#if DEBUG
        if (ButtonsPanel != null)
        {
            ButtonsPanel.Name ??= "buttonsPanel";
        }
#endif
        if (ButtonsPanel != null)
        {
            BackPanel.Children.Add(ButtonsPanel);
        }
    }

    /// <summary>
    /// Gets or sets the dialog title text shown by the <see cref="TitleBar"/> when available.
    /// </summary>
    /// <remarks>
    /// Returns an empty string when no <see cref="TitleBar"/> or title text visual is present.
    /// Setting this property is a no-op when no title text visual exists.
    /// </remarks>
    [Category(CategoryBehavior)]
    public string Title
    {
        get => TitleBar?.Title?.Text ?? string.Empty;
        set
        {
            var title = TitleBar?.Title;
            if (title != null)
            {
                title.Text = value;
            }
        }
    }

    /// <summary>
    /// Enumerates the <see cref="Button"/> instances contained in <see cref="ButtonsPanel"/>.
    /// </summary>
    /// <remarks>
    /// Returns an empty sequence when <see cref="ButtonsPanel"/> is null.
    /// </remarks>
    [Category(CategoryBehavior)]
    public IEnumerable<Button> Buttons => ButtonsPanel?.Children.OfType<Button>() ?? [];

    /// <summary>
    /// Gets the optional title bar, when created by <see cref="CreateTitleBar"/>.
    /// </summary>
    [Browsable(false)]
    public TitleBar? TitleBar { get; }

    /// <summary>
    /// Gets the required back panel hosting the title bar, dialog content, and buttons panel.
    /// </summary>
    [Browsable(false)]
    public Visual BackPanel { get; }

    /// <summary>
    /// Gets the required dialog content area (default: a <see cref="Dock"/>).
    /// </summary>
    [Browsable(false)]
    public Visual DialogContent { get; }

    /// <summary>
    /// Gets the optional buttons host (default: a right-docking <see cref="Dock"/>), or null when disabled.
    /// </summary>
    [Browsable(false)]
    public Visual? ButtonsPanel { get; }

    /// <summary>
    /// Gets the optional decorative border (default: a white <see cref="RoundedRectangle"/>), or null when disabled.
    /// </summary>
    [Browsable(false)]
    public Visual? Border { get; }

    /// <summary>
    /// Creates an optional border behind the dialog content.
    /// </summary>
    /// <returns>
    /// A <see cref="RoundedRectangle"/> with a theme-adjusted corner radius and white brush,
    /// or null to disable the border.
    /// </returns>
    /// <remarks>
    /// The brush assignment is deferred until attached to composition using <see cref="Visual.DoWhenAttachedToComposition(Action, VisualDoOptions)"/>.
    /// </remarks>
    protected virtual Visual? CreateBorder()
    {
        var border = new RoundedRectangle { CornerRadius = new Vector2(GetWindowTheme().RoundedButtonCornerRadius) };
        border.DoWhenAttachedToComposition(() => border.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.White.ToColor()));
        return border;
    }

    /// <summary>
    /// Creates the required dialog content host.
    /// </summary>
    /// <returns>
    /// A <see cref="Dock"/> with theme margin and centered vertical alignment.
    /// </returns>
    protected virtual Visual CreateDialogContent()
    {
        var dock = new Dock
        {
            Margin = GetWindowTheme().ButtonMargin,
            VerticalAlignment = Alignment.Center
        };
        return dock;
    }

    /// <summary>
    /// Creates an optional title bar.
    /// </summary>
    /// <returns>
    /// A configured <see cref="TitleBar"/> with hidden Min/Max buttons and a Close handler that sets <see cref="Dialog.Result"/> to false and closes,
    /// or null to disable the title bar.
    /// </returns>
    /// <remarks>
    /// When present, the title text is given theme margins and default font size.
    /// </remarks>
    protected virtual TitleBar? CreateTitleBar()
    {
        var bar = new TitleBar();
        if (bar.Title != null)
        {
            var margin = GetWindowTheme().ButtonMargin;
            bar.Title.FontSize = GetWindowTheme().DefaultFontSize;
            bar.Title.Margin = D2D_RECT_F.Thickness(margin, 0, margin, 0);
        }

        if (bar.MaxButton != null)
        {
            bar.MaxButton.IsVisible = false;
        }

        if (bar.MinButton != null)
        {
            bar.MinButton.IsVisible = false;
        }

        if (bar.CloseButton is IClickable clickable)
        {
            clickable.Click += (s2, e2) => { Result = false; if (TryClose()) Close(); };
        }
        return bar;
    }

    /// <summary>
    /// Creates the required back panel that stacks child visuals vertically.
    /// </summary>
    /// <returns>A vertical <see cref="Stack"/> whose last child fills the remaining space.</returns>
    /// <remarks>
    /// The panel synchronizes the dialog's <see cref="Visual.Width"/> and <see cref="Visual.Height"/>
    /// to its arranged size after layout (with a small threshold to avoid feedback loops).
    /// </remarks>
    protected virtual Visual CreateBackPanel()
    {
        var panel = new Stack() { Orientation = Orientation.Vertical, LastChildFill = true };
        panel.Arranged += (s, e) =>
        {
            var ar = panel.ArrangedRect;

            // avoid infinite loops
            const float minDiff = 0.01f;
            if (Math.Abs(Width - ar.Width) > minDiff)
            {
                Width = ar.Width;
            }

            if (Math.Abs(Height - ar.Height) > minDiff)
            {
                Height = ar.Height;
            }
        };
        return panel;
    }

    /// <summary>
    /// Creates the optional buttons panel.
    /// </summary>
    /// <returns>
    /// A <see cref="Dock"/> with <see cref="Dock.LastChildFill"/> disabled and theme margin, or null to disable buttons.
    /// </returns>
    protected virtual Visual? CreateButtonsPanel() => new Dock() { LastChildFill = false, Margin = GetWindowTheme().ButtonMargin };

    /// <summary>
    /// Creates a new themed button instance.
    /// </summary>
    /// <returns>A new <see cref="Button"/>.</returns>
    protected virtual Button CreateButton() => new();

    /// <summary>
    /// Returns the first button whose <see cref="Button.Command"/> matches the provided message box result.
    /// </summary>
    /// <param name="command">The command/result to look for.</param>
    /// <returns>The matching <see cref="Button"/>, or null when not found.</returns>
    public Button? GetCommandButton(MESSAGEBOX_RESULT command) => Buttons.FirstOrDefault(b => command.Equals(b.Command));

    /// <summary>
    /// Adds a new button to the <see cref="ButtonsPanel"/>.
    /// </summary>
    /// <returns>The newly created button.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="ButtonsPanel"/> is null.</exception>
    /// <remarks>
    /// - Applies theme margins and font size, centers the text, and docks to the right.<br/>
    /// - Note: buttons must be inserted in reverse order due to right docking.
    /// </remarks>
    public virtual Button AddButton()
    {
        if (ButtonsPanel == null)
            throw new InvalidOperationException();

        var button = CreateButton();
        var margin = GetWindowTheme().ButtonMargin;
        button.Margin = D2D_RECT_F.Thickness(margin, 0, 0, 0);

        button.Text.Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER;
        button.Text.FontSize = GetWindowTheme().DialogBoxButtonFontSize;
        Dock.SetDockType(button, DockType.Right); // note buttons must be inserted in reverse order
        ButtonsPanel.Children.Add(button);
        return button;
    }

    /// <summary>
    /// Adds a button bound to a specific <paramref name="command"/> and optionally wires a dialog result.
    /// </summary>
    /// <param name="command">The message box result/command associated with the button.</param>
    /// <param name="result">
    /// Optional dialog result to set when the button is clicked; when provided, the dialog attempts to close.
    /// </param>
    /// <param name="accessKeys">Optional access keys (e.g., Enter/Escape) to invoke the button.</param>
    /// <returns>The newly created button.</returns>
    /// <remarks>
    /// The button text is resolved using <see cref="WiceCommons.GetMessageBoxString(MESSAGEBOX_RESULT)"/>.
    /// </remarks>
    public virtual Button AddCommandButton(MESSAGEBOX_RESULT command, bool? result = null, params AccessKey[] accessKeys)
    {
        var button = AddButton();
#if DEBUG
        button.Name ??= command + "Button";
#endif
        button.Text.Text = WiceCommons.GetMessageBoxString(command) ?? string.Empty;
        button.Command = command;
        button.AccessKeys.AddRange(accessKeys);

        if (result.HasValue)
        {
            button.Click += (s, e) => { Result = result.Value; if (TryClose()) Close(); };
        }
        return button;
    }

    /// <summary>Adds a default OK button (Enter) that closes the dialog with <c>true</c>.</summary>
    public virtual Button AddOkButton() => AddCommandButton(MESSAGEBOX_RESULT.IDOK, true, AccessKey.Enter);

    /// <summary>Adds a default Cancel button (Escape) that closes the dialog with <c>false</c>.</summary>
    public virtual Button AddCancelButton() => AddCommandButton(MESSAGEBOX_RESULT.IDCANCEL, false, AccessKey.Escape);

    /// <summary>Adds a default Yes button (Enter) that closes the dialog with <c>true</c>.</summary>
    public virtual Button AddYesButton() => AddCommandButton(MESSAGEBOX_RESULT.IDYES, true, AccessKey.Enter);

    /// <summary>Adds a default No button (Escape) that closes the dialog with <c>false</c>.</summary>
    public virtual Button AddNoButton() => AddCommandButton(MESSAGEBOX_RESULT.IDNO, false, AccessKey.Escape);

    /// <summary>Adds a default Close button (Enter/Escape) that closes the dialog with <c>false</c>.</summary>
    public virtual Button AddCloseButton() => AddCommandButton(MESSAGEBOX_RESULT.IDCLOSE, false, AccessKey.Enter, AccessKey.Escape);

    /// <summary>
    /// Subscribes to theme/DPI changes and initializes DPI-dependent properties after attaching to composition.
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
    /// Unsubscribes from theme/DPI changes when detaching from composition.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event data.</param>
    protected override void OnDetachingFromComposition(object? sender, EventArgs e)
    {
        base.OnDetachingFromComposition(sender, e);
        Window!.ThemeDpiEvent -= OnThemeDpiEvent;
    }

    /// <summary>
    /// Applies theme/DPI updates to margins, title bar, rounded corners, and button font sizes.
    /// </summary>
    /// <param name="sender">Event source (typically the owning <see cref="Window"/>).</param>
    /// <param name="e">DPI event data.</param>
    protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        DialogContent.Margin = GetWindowTheme().ButtonMargin;
        if (Border is RoundedRectangle roundedRectangle)
        {
            roundedRectangle.CornerRadius = new Vector2(GetWindowTheme().RoundedButtonCornerRadius);
        }
        TitleBar?.Update();

        if (ButtonsPanel != null)
        {
            var margin = GetWindowTheme().ButtonMargin;
            ButtonsPanel.Margin = margin;
            foreach (var button in ButtonsPanel.Children.OfType<Button>())
            {
                button.Margin = D2D_RECT_F.Thickness(margin, 0, 0, 0);
                button.Text.FontSize = GetWindowTheme().DialogBoxButtonFontSize;
            }
        }
    }
}
