namespace Wice;

public abstract class HwndVisual : Visual
{
    private NativeWindow? _nativeWindow;
    private HWND _handle;

    public NativeWindow? NativeWindow => _nativeWindow;
    public HWND Handle
    {
        get => _handle;
        protected set
        {
            if (_handle == value)
                return;

            _handle = value;
            _nativeWindow = NativeWindow.FromHandle(value);
            if (_nativeWindow != null)
            {
                _nativeWindow.ManagedThreadId = Environment.CurrentManagedThreadId;
            }
        }
    }

    protected abstract HWND CreateWindow(HWND parent);

    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        Window!.WindowMessage += OnWindowMessage;
        Handle = CreateWindow(Window.Handle);
    }

    protected override void OnDetachingFromComposition(object? sender, EventArgs e)
    {
        base.OnDetachingFromComposition(sender, e);
        Window!.WindowMessage -= OnWindowMessage;
        Functions.DestroyWindow(Handle);
        Handle = HWND.Null;
    }

    protected virtual void OnWindowMessage(object? sender, WindowMessageEventArgs e)
    {
    }
}
