namespace Wice;

/// <summary>
/// Defines the contract for an entity that hosts a <see cref="Viewer"/> and exposes
/// scroll-related state, including scrollbar visibility and current scroll offsets.
/// </summary>
public interface IViewerParent
{
    /// <summary>
    /// Gets the <see cref="Wice.Viewer"/> associated with this parent.
    /// </summary>
    /// <value>
    /// The non-null viewer instance that this parent is hosting.
    /// </value>
    Viewer Viewer { get; }

    /// <summary>
    /// Gets a value indicating whether the vertical scroll bar is currently visible.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a vertical scroll bar is visible; otherwise, <see langword="false"/>.
    /// </value>
    bool IsVerticalScrollBarVisible { get; }

    /// <summary>
    /// Gets a value indicating whether the horizontal scroll bar is currently visible.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a horizontal scroll bar is visible; otherwise, <see langword="false"/>.
    /// </value>
    bool IsHorizontalScrollBarVisible { get; }

    /// <summary>
    /// Gets or sets the horizontal scroll offset in device-independent pixels (DIPs).
    /// </summary>
    /// <value>
    /// The horizontal offset where <c>0</c> represents the leftmost position. Implementations should clamp
    /// this value to the valid scrollable range.
    /// </value>
    float HorizontalOffset { get; set; }

    /// <summary>
    /// Gets or sets the vertical scroll offset in device-independent pixels (DIPs).
    /// </summary>
    /// <value>
    /// The vertical offset where <c>0</c> represents the topmost position. Implementations should clamp
    /// this value to the valid scrollable range.
    /// </value>
    float VerticalOffset { get; set; }
}
