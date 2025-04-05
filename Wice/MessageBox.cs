using System;
using DirectN;

namespace Wice
{
    public class MessageBox : DialogBox
    {
        protected DialogResult? MessageBoxResult { get; set; }

        public new DialogResult Result
        {
            get
            {
                if (MessageBoxResult.HasValue)
                    return MessageBoxResult.Value;

                if (base.Result == true)
                    return DialogResult.OK;

                if (base.Result == false)
                    return DialogResult.Cancel;

                return DialogResult.None;
            }
        }

        public static void Show(Window window, string text, string title = null, Action<MessageBox> onClose = null)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var dlg = new MessageBox { Title = title ?? Application.GetTitle(window.Handle) };
            var button = dlg.AddCloseButton();
            button.Click += (s, e) => dlg.MessageBoxResult = DialogResult.OK;

            var tv = new TextBox();
#if DEBUG
            tv.Name = "messageBoxText";
#endif
            tv.Padding = D2D_RECT_F.Thickness(5);
            tv.Text = text;
            dlg.DialogContent.Children.Add(tv);
            window.Children.Add(dlg);

            if (onClose != null)
            {
                dlg.DoWhenDetachedFromParent(() => onClose(dlg));
            }
        }
    }
}
