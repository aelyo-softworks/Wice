namespace Wice;

public partial class DialogBox : Dialog
{
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

    [Category(CategoryBehavior)]
    public IEnumerable<Button> Buttons => ButtonsPanel?.Children.OfType<Button>() ?? [];

    [Browsable(false)]
    public TitleBar? TitleBar { get; }

    [Browsable(false)]
    public Visual BackPanel { get; }

    [Browsable(false)]
    public Visual DialogContent { get; }

    [Browsable(false)]
    public Visual? ButtonsPanel { get; }

    [Browsable(false)]
    public Visual? Border { get; }

    protected virtual Visual? CreateBorder()
    {
        var border = new RoundedRectangle { CornerRadius = new Vector2(Application.CurrentTheme.RoundedButtonCornerRadius) };
        border.DoWhenAttachedToComposition(() => border.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.White.ToColor()));
        return border;
    }

    protected virtual Visual CreateDialogContent()
    {
        var dock = new Dock
        {
            Margin = Application.CurrentTheme.ButtonMargin,
            VerticalAlignment = Alignment.Center
        };
        return dock;
    }

    protected virtual TitleBar? CreateTitleBar()
    {
        var bar = new TitleBar();
        if (bar.Title != null)
        {
            var margin = Application.CurrentTheme.ButtonMargin;
            bar.Title.FontSize = Application.CurrentTheme.DefaultFontSize;
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

    protected virtual Visual? CreateButtonsPanel() => new Dock() { LastChildFill = false, Margin = Application.CurrentTheme.ButtonMargin };
    protected virtual Button CreateButton() => new();

    public Button? GetCommandButton(MESSAGEBOX_RESULT command) => Buttons.FirstOrDefault(b => command.Equals(b.Command));

    public virtual Button AddButton()
    {
        if (ButtonsPanel == null)
            throw new InvalidOperationException();

        var button = CreateButton();
        var margin = Application.CurrentTheme.ButtonMargin;
        button.Margin = D2D_RECT_F.Thickness(margin, 0, 0, 0);

        button.Text.Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER;
        button.Text.FontSize = Application.CurrentTheme.DialogBoxButtonFontSize;
        Dock.SetDockType(button, DockType.Right); // note buttons must be inserted in reverse order
        ButtonsPanel.Children.Add(button);
        return button;
    }

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

    public virtual Button AddOkButton() => AddCommandButton(MESSAGEBOX_RESULT.IDOK, true, AccessKey.Enter);
    public virtual Button AddCancelButton() => AddCommandButton(MESSAGEBOX_RESULT.IDCANCEL, false, AccessKey.Escape);
    public virtual Button AddYesButton() => AddCommandButton(MESSAGEBOX_RESULT.IDYES, true, AccessKey.Enter);
    public virtual Button AddNoButton() => AddCommandButton(MESSAGEBOX_RESULT.IDNO, false, AccessKey.Escape);
    public virtual Button AddCloseButton() => AddCommandButton(MESSAGEBOX_RESULT.IDCLOSE, false, AccessKey.Enter, AccessKey.Escape);
}
