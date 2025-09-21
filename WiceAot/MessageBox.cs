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

    /// <summary>
    /// Applies theme/DPI updates to the message text padding.
    /// </summary>
    /// <param name="sender">The event source (typically the owning <see cref="Window"/>).</param>
    /// <param name="e">DPI event data.</param>
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
    /// Shows a modal message dialog attached to the specified <paramref name="window"/>.
    /// </summary>
    /// <param name="window">The owning window. Must not be null.</param>
    /// <param name="text">The message text to display. Must not be null.</param>
    /// <param name="title">
    /// Optional dialog title. When not provided, the window title is used via <see cref="Application.GetTitle(nint)"/>.
    /// </param>
    /// <param name="onClose">
    /// Optional callback invoked after the dialog is removed from its parent. Receives the <see cref="MessageBox"/> instance
    /// whose <see cref="Result"/> reflects the outcome at close time.
    /// </param>
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
