namespace Wice.Samples.Gallery.Samples.Misc.DragDrop;

public class DropSourceSample : Sample
{
    public override string Description => "A visual that you can drag and drop in any drop target window, Windows explorer etc.";

    public override void Layout(Visual parent)
    {
        var stack = new Stack { Orientation = Orientation.Horizontal };
        Dock.SetDockType(stack, DockType.Top); // remove from display
        parent.Children.Add(stack); // remove from display

        var btn = new Button();
        btn.Text.Text = "Drag me out of here!";
        stack.Children.Add(btn);

        var info = new Stack { Orientation = Orientation.Vertical }; // remove from display
        stack.Children.Add(info);// remove from display

        var target = new TextBox { VerticalAlignment = Alignment.Near, Padding = 10 };
        info.Children.Add(target);

        btn.MouseButtonDown += (s, e) =>
        {
            if (e.Button == MouseButton.Left)
            {
                // create an IDataObject from the gallery.exe file
#if NETFRAMEWORK
                using (var dataObject = ShellUtilities.CreateDataObject(new[] { Process.GetCurrentProcess().MainModule.FileName }))
                {
                    // optionally hook events to be notified of windows being dragged over
                    Window!.DragDropTarget += (s2, e2) =>
                    {
                        if (e2.Type == DragDropTargetEventType.Leave)
                        {
                            target.Text = string.Empty;
                        }
                        else
                        {
                            var window = NativeWindow.FromHandle(e2.Hwnd);
                            target.Text = $"Target Window: {e2.Hwnd.Value.ToHexa()} '{window?.Text}' process: '{window?.Process?.ProcessName}'";
                        }
                    };

                    btn.DoDragDrop(dataObject.Object, DROPEFFECT.DROPEFFECT_COPY);
                }
#else
                using var dataObject = ShellUtilities.CreateDataObject([Environment.ProcessPath!]);

                // optionally hook events to be notified of windows being dragged over
                Window!.DragDropTarget += (s, e) =>
                {
                    if (e.Type == DragDropTargetEventType.Leave)
                    {
                        target.Text = string.Empty;
                    }
                    else
                    {
                        var window = NativeWindow.FromHandle(e.Hwnd);
                        target.Text = $"Target Window: {((uint)e.Hwnd.Value).ToHex()} '{window?.Text}' process: '{window?.Process?.ProcessName}'";
                    }
                };

                btn.DoDragDrop(dataObject.NativeObject, DROPEFFECT.DROPEFFECT_COPY);
#endif
            }
        };
    }
}
