namespace Wice;

/// <summary>
/// Base visual that hosts a native child window (<see cref="HWND"/>) inside the Wice composition tree.
/// </summary>
public abstract class HwndVisual : Visual
{
    private NativeWindow? _nativeWindow;
    private HWND _handle;

    /// <summary>
    /// Gets the managed wrapper for the current native <see cref="HWND"/>.
    /// Returns <see langword="null"/> if no handle is associated (not created or already destroyed).
    /// </summary>
    public NativeWindow? NativeWindow => _nativeWindow;

    /// <summary>
    /// Gets the native window handle owned by this visual.
    /// Returns <see cref="HWND.Null"/> when the handle has not been created or has been destroyed.
    /// </summary>
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

    /// <summary>
    /// Creates the child <see cref="HWND"/> hosted by this visual.
    /// </summary>
    /// <param name="parent">The parent window handle, typically <see cref="Window.Handle"/>.</param>
    /// <returns>
    /// The created child window handle. Should be a valid <see cref="HWND"/>. Return <see cref="HWND.Null"/> on failure.
    /// </returns>
    protected abstract HWND CreateWindow(HWND parent);

    /// <summary>
    /// Called when this visual is attached to the composition (the owning <see cref="Window"/> exists).
    /// Subscribes to window messages and creates the child <see cref="HWND"/> via <see cref="CreateWindow(HWND)"/>.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Event data.</param>
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        Window!.WindowMessage += OnWindowMessage;
        Handle = CreateWindow(Window.Handle);
    }

    /// <summary>
    /// Called when this visual is detaching from the composition.
    /// Unsubscribes from window messages, destroys the child <see cref="HWND"/>, and clears <see cref="Handle"/>.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Event data.</param>
    protected override void OnDetachingFromComposition(object? sender, EventArgs e)
    {
        base.OnDetachingFromComposition(sender, e);
        Window!.WindowMessage -= OnWindowMessage;
        Functions.DestroyWindow(Handle);
        Handle = HWND.Null;
    }

    /// <summary>
    /// Called for each native message received by the owning <see cref="Window"/>.
    /// Derived classes can override to observe or handle messages relevant to the hosted child window.
    /// </summary>
    /// <param name="sender">The window raising the event.</param>
    /// <param name="e">The native message event arguments.</param>
    protected virtual void OnWindowMessage(object? sender, WindowMessageEventArgs e)
    {
    }
}
