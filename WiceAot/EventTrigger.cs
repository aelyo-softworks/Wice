namespace Wice;

/// <summary>
/// Defines when a value change or user interaction should trigger an update or action.
/// </summary>
/// <remarks>
/// <see cref="Default"/> and <see cref="ValueChanged"/> are equivalent in this implementation.
/// Choose <see cref="LostFocus"/> or <see cref="LostFocusOrReturnPressed"/> to defer updates until
/// the control loses focus or the user confirms with the Return/Enter key.
/// </remarks>
public enum EventTrigger
{
    /// <summary>
    /// Uses the default trigger behavior. Equivalent to <see cref="ValueChanged"/>.
    /// </summary>
    Default, // Default and ValueChanged are the same here

    /// <summary>
    /// Triggers as soon as the underlying value changes (e.g., on text change).
    /// </summary>
    ValueChanged,

    /// <summary>
    /// Triggers when the control loses keyboard focus.
    /// </summary>
    LostFocus,

    /// <summary>
    /// Triggers when the control loses focus or when the Return/Enter key is pressed.
    /// </summary>
    LostFocusOrReturnPressed,
}
