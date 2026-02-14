namespace Wice;

/// <summary>
/// Provides event data for mouse button input (down, up, double-click).
/// </summary>
public class MouseButtonEventArgs(int x, int y, POINTER_MOD vk, MouseButton button)
    : MouseEventArgs(x, y, vk)
{
    /// <summary>
    /// Gets the mouse button associated with this event.
    /// </summary>
    public MouseButton Button { get; } = button;

    /// <summary>
    /// Gets or sets the initial delay, in milliseconds, before auto-repeat begins
    /// for a mouse button down event.
    /// </summary>
    public virtual uint RepeatDelay { get; set; } // if > 0, for mouse button down events only

    /// <summary>
    /// Gets or sets the interval, in milliseconds, between successive repeat notifications
    /// after <see cref="RepeatDelay"/> has elapsed for a button down event.
    /// </summary>
    public virtual uint RepeatInterval { get; set; } // if > 0, for mouse button down events only

    /// <inheritdoc/>
    public override string ToString() => $"{base.ToString()} B:{Button} RptD:{RepeatDelay} RptI:{RepeatInterval}";
}
