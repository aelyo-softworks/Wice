namespace Wice;

public partial class MessageBox : DialogBox
{
    protected MESSAGEBOX_RESULT? MessageBoxResult { get; set; }

    public new MESSAGEBOX_RESULT Result
    {
        get
        {
            if (MessageBoxResult.HasValue)
                return MessageBoxResult.Value;

            if (base.Result == true)
                return MESSAGEBOX_RESULT.IDOK;

            if (base.Result == false)
                return MESSAGEBOX_RESULT.IDCANCEL;

            return 0;
        }
    }

    protected override void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        base.OnThemeDpiEvent(sender, e);
        var theme = GetWindowTheme();
        foreach (var tb in DialogContent.Children.OfType<MessageTextBox>())
        {
            tb.Padding = D2D_RECT_F.Thickness(theme.MessageBoxPadding);
        }
    }

    public static void Show(Window window, string text, string? title = null, Action<MessageBox>? onClose = null)
    {
        ExceptionExtensions.ThrowIfNull(window, nameof(window));
        ExceptionExtensions.ThrowIfNull(text, nameof(text));
        var dlg = new MessageBox { Title = title ?? Application.GetTitle(window.Handle) };
        var button = dlg.AddCloseButton();
        button.Click += (s, e) => dlg.MessageBoxResult = MESSAGEBOX_RESULT.IDOK;

        var tb = new MessageTextBox
        {
#if DEBUG
            Name = "messageBoxText",
#endif
            Text = text
        };

        dlg.DialogContent.Children.Add(tb);
        window.Children.Add(dlg);

        if (onClose != null)
        {
            dlg.DoWhenDetachedFromParent(() => onClose(dlg));
        }
    }

    private sealed partial class MessageTextBox : TextBox
    {
    }
}
