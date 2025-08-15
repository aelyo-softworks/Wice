namespace Wice;

/// <summary>
/// Specifies the direction to move input focus during keyboard or programmatic navigation.
/// </summary>
/// <remarks>
/// Use this value to advance or reverse focus through the tab order or custom navigation logic.
/// </remarks>
public enum FocusDirection
{
    /// <summary>
    /// Move focus to the next focusable element in the tab order.
    /// </summary>
    Next,

    /// <summary>
    /// Move focus to the previous focusable element in the tab order.
    /// </summary>
    Previous,
}
