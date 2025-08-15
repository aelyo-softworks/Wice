namespace Wice;

/// <summary>
/// Defines the visibility and behavior of scroll bars for a scrollable viewport.
/// </summary>
/// <remarks>
/// Semantics are aligned with common UI frameworks:
/// - Disabled: no scrolling and no scroll bars.
/// - Auto: show scroll bars only when content overflows.
/// - Hidden: scrolling allowed without visible scroll bars.
/// - Visible: scroll bars are always shown.
/// </remarks>
public enum ScrollBarVisibility
{
    /// <summary>
    /// Scrolling is disabled and no scroll bar is shown.
    /// </summary>
    Disabled,

    /// <summary>
    /// The scroll bar is shown only when the content exceeds the viewport; scrolling is enabled as needed.
    /// </summary>
    Auto,

    /// <summary>
    /// Scrolling is enabled but the scroll bar is not visible (e.g., scroll via mouse wheel, touch, or keyboard).
    /// </summary>
    Hidden,

    /// <summary>
    /// The scroll bar is always visible; scrolling is enabled regardless of content size.
    /// </summary>
    Visible,
}
