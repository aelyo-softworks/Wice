namespace Wice;

/// <summary>
/// Provides event data for a native Win32 window message dispatched to an HWND.
/// </summary>
/// <param name="handle">The window handle that received the message.</param>
/// <param name="message">The numeric message identifier (e.g., WM_*).</param>
/// <param name="wParam">The WPARAM value associated with the message.</param>
/// <param name="lParam">The LPARAM value associated with the message.</param>
public class WindowMessageEventArgs(HWND handle, uint message, WPARAM wParam, LPARAM lParam) : HandledEventArgs
{
    /// <summary>
    /// Gets the window handle (HWND) that received the message.
    /// </summary>
    public HWND Handle { get; } = handle;

    /// <summary>
    /// Gets the message identifier (WM_*).
    /// </summary>
    public uint Message { get; } = message;

    /// <summary>
    /// Gets the WPARAM value for the message.
    /// </summary>
    public WPARAM WParam { get; } = wParam;

    /// <summary>
    /// Gets the LPARAM value for the message.
    /// </summary>
    public LPARAM LParam { get; } = lParam;

    /// <summary>
    /// Gets or sets the result that should be returned to the window procedure when the event is handled.
    /// </summary>
    public virtual LRESULT Result { get; set; }

    /// <inheritdoc/>
    public override string ToString() => WiceCommons.DecodeMessage(Handle, Message, WParam, LParam);
}
