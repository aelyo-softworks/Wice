namespace Wice;

/// <summary>
/// Provides event data for mouse button input (down, up, double-click).
/// </summary>
/// <remarks>
/// Inherits window-relative coordinates and modifier/button state from <see cref="MouseEventArgs"/>.
/// For button-down events, setting <see cref="RepeatDelay"/> and <see cref="RepeatInterval"/> enables
/// auto-repeat behavior when supported by the input pipeline.
/// </remarks>
/// <param name="x">The X coordinate in window-space units.</param>
/// <param name="y">The Y coordinate in window-space units.</param>
/// <param name="vk">The modifier/button state flags associated with the event.</param>
/// <param name="button">The mouse button that changed state for this event.</param>
/// <seealso cref="MouseEventArgs"/>
/// <seealso cref="MouseButton"/>
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
    /// <remarks>
    /// Applies only to mouse button down events. A value of 0 disables repeat.
    /// </remarks>
    public virtual uint RepeatDelay { get; set; } // if > 0, for mouse button down events only

    /// <summary>
    /// Gets or sets the interval, in milliseconds, between successive repeat notifications
    /// after <see cref="RepeatDelay"/> has elapsed for a button down event.
    /// </summary>
    /// <remarks>
    /// Applies only to mouse button down events. A value of 0 disables repeat.
    /// </remarks>
    public virtual uint RepeatInterval { get; set; } // if > 0, for mouse button down events only
}
