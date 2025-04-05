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

    public static void Show(Window window, string text, string? title = null, Action<MessageBox>? onClose = null)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(text);
        var dlg = new MessageBox { Title = title ?? Application.GetTitle(window.Handle) };
        var button = dlg.AddCloseButton();
        button.Click += (s, e) => dlg.MessageBoxResult = MESSAGEBOX_RESULT.IDOK;

        var tv = new TextBox
        {
#if DEBUG
            Name = "messageBoxText",
#endif
            Padding = D2D_RECT_F.Thickness(5),
            Text = text
        };
        dlg.DialogContent.Children.Add(tv);
        window.Children.Add(dlg);

        if (onClose != null)
        {
            dlg.DoWhenDetachedFromParent(() => onClose(dlg));
        }
    }
}
