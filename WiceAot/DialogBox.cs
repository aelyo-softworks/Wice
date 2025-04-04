﻿namespace Wice;

public partial class DialogBox : Dialog
{
    private readonly RoundedRectangle _rr;

    public DialogBox()
    {
        _rr = new RoundedRectangle
        {
            CornerRadius = new Vector2(Application.CurrentTheme.RoundedButtonCornerRadius)
        };

        _rr.DoWhenAttachedToComposition(() => _rr.RenderBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.White.ToColor()));
        Content.Children.Add(_rr);

        BackPanel = CreateBackPanel();
        if (BackPanel == null)
            throw new InvalidOperationException();

#if DEBUG
        BackPanel.Name = "backPanel";
#endif
        Content.Children.Add(BackPanel);
        BackPanel.Arranged += (s, e) =>
        {
            var ar = BackPanel.ArrangedRect;

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

        TitleBar = CreateTitleBar();
        if (TitleBar == null)
            throw new InvalidOperationException();
#if DEBUG
        TitleBar.Name = "titleBar";
#endif

        var margin = Application.CurrentTheme.ButtonMargin;
        if (TitleBar.Title != null)
        {
            TitleBar.Title.FontSize = Application.CurrentTheme.DefaultFontSize;
            TitleBar.Title.Margin = D2D_RECT_F.Thickness(margin, 0, margin, 0);
        }

        if (TitleBar.MaxButton != null)
        {
            TitleBar.MaxButton.IsVisible = false;
        }

        if (TitleBar.MinButton != null)
        {
            TitleBar.MinButton.IsVisible = false;
        }

        if (TitleBar.CloseButton is IClickable clickable)
        {
            clickable.Click += (s2, e2) => { Result = false; if (TryClose()) Close(); };
        }

        BackPanel.Children.Add(TitleBar);

        DialogContent = CreateDialogContent();
        DialogContent.Margin = margin;
        DialogContent.VerticalAlignment = Alignment.Center;
        if (DialogContent == null)
            throw new InvalidOperationException();
#if DEBUG
        DialogContent.Name = "dialogContent";
#endif
        BackPanel.Children.Add(DialogContent);

        ButtonsPanel = CreateButtonsPanel();
        if (ButtonsPanel == null)
            throw new InvalidOperationException();
#if DEBUG
        ButtonsPanel.Name = "buttonsPanel";
#endif
        ButtonsPanel.Margin = margin;
        BackPanel.Children.Add(ButtonsPanel);
    }

    [Category(CategoryBehavior)]
    public string Title
    {
        get => TitleBar.Title?.Text ?? string.Empty;
        set
        {
            var title = TitleBar.Title;
            if (title != null)
            {
                title.Text = value;
            }
        }
    }

    [Category(CategoryBehavior)]
    public IEnumerable<Button> Buttons => ButtonsPanel?.Children.OfType<Button>() ?? [];

    [Browsable(false)]
    public TitleBar TitleBar { get; }

    [Browsable(false)]
    public Visual BackPanel { get; }

    [Browsable(false)]
    public Visual DialogContent { get; }

    [Browsable(false)]
    public Visual ButtonsPanel { get; }

    protected virtual Visual CreateDialogContent() => new Dock();
    protected virtual TitleBar CreateTitleBar() => new();
    protected virtual Visual CreateBackPanel() => new Stack() { Orientation = Orientation.Vertical, LastChildFill = true };
    protected virtual Visual CreateButtonsPanel() => new Dock() { LastChildFill = false };
    protected virtual Button CreateButton() => new();

    public Button? GetCommandButton(MESSAGEBOX_RESULT command) => Buttons.FirstOrDefault(b => command.Equals(b.Command));

    public virtual Button AddButton()
    {
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
        button.Name = command + "Button";
#endif
        button.Text.Text = Functions.GetMessageBoxString(command) ?? string.Empty;
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
