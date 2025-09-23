namespace Wice;

/// <summary>
/// Simple modal message dialog built on top of <see cref="DialogBox"/>.
/// </summary>
public partial class MessageBox : DialogBox
{
    /// <summary>
    /// Holds an explicit message box result chosen by command buttons.
    /// When set, it takes precedence over the base <see cref="Dialog.Result"/> mapping.
    /// </summary>
    protected MESSAGEBOX_RESULT? MessageBoxResult { get; set; }

    /// <summary>
    /// Gets the effective message box result.
    /// </summary>
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

    /// <inheritdoc/>
    protected override void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        base.OnThemeDpiEvent(sender, e);
        var theme = GetWindowTheme();
        foreach (var tb in DialogContent.Children.OfType<MessageTextBox>())
        {
            tb.Padding = D2D_RECT_F.Thickness(theme.MessageBoxPadding);
        }
    }

    /// <summary>
    /// Displays a message box with the specified text and optional title, attached to the given window.
    /// </summary>
    /// <param name="window">The parent <see cref="Window"/> to which the message box will be attached. Cannot be <see langword="null"/>.</param>
    /// <param name="text">The message text to display in the message box. Cannot be <see langword="null"/>.</param>
    /// <param name="title">The optional title of the message box. If <see langword="null"/>, the title is derived from the parent window.</param>
    /// <param name="onClose">An optional callback to execute when the message box is closed. The callback receives the <see
    /// cref="MessageBox"/> instance as a parameter.</param>
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

    // Private text host used as the message body inside the dialog content.
    private sealed partial class MessageTextBox : TextBox
    {
    }
}
