namespace Wice;

/// <summary>
/// Simple modal message dialog built on top of <see cref="DialogBox"/>.
/// </summary>
/// <remarks>
/// Behavior:
/// - Hosts a single text element (<see cref="MessageTextBox"/>) inside <see cref="DialogBox.DialogContent"/>.
/// - Adds a Close/OK button and stores <see cref="MESSAGEBOX_RESULT.IDOK"/> when it is clicked.
/// - Maps the underlying <see cref="Dialog.Result"/> (true/false) to <see cref="MESSAGEBOX_RESULT.IDOK"/>/<see cref="MESSAGEBOX_RESULT.IDCANCEL"/> when
///   no explicit <see cref="MessageBoxResult"/> has been set.
/// - Updates message padding on theme/DPI changes using <c>theme.MessageBoxPadding</c>.
/// </remarks>
/// <seealso cref="DialogBox"/>
/// <seealso cref="Button"/>
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
    /// <remarks>
    /// Precedence:
    /// 1) When <see cref="MessageBoxResult"/> is set, returns that value.
    /// 2) Otherwise maps <see cref="Dialog.Result"/>: true → <see cref="MESSAGEBOX_RESULT.IDOK"/>, false → <see cref="MESSAGEBOX_RESULT.IDCANCEL"/>.
    /// 3) Returns 0 when neither source provides a result (e.g., dialog not closed yet).
    /// </remarks>
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
    /// <remarks>
    /// Iterates all <see cref="MessageTextBox"/> instances found in <see cref="DialogBox.DialogContent"/> and
    /// sets their <see cref="TextBox.Padding"/> using <c>theme.MessageBoxPadding</c>.
    /// </remarks>
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
    /// <remarks>
    /// Implementation details:
    /// - Creates a <see cref="MessageTextBox"/> with the provided <paramref name="text"/> and inserts it into <see cref="DialogBox.DialogContent"/>.
    /// - Adds a Close/OK button via <see cref="DialogBox.AddCloseButton"/> and stores <see cref="MESSAGEBOX_RESULT.IDOK"/> when it is clicked.
    /// - Attaches the dialog to the window; <paramref name="onClose"/> is scheduled using <see cref="Visual.DoWhenDetachedFromParent(System.Action, VisualDoOptions)"/>.
    /// </remarks>
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
