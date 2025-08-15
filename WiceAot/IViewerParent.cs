namespace Wice;

/// <summary>
/// Defines the contract for an entity that hosts a <see cref="Viewer"/> and exposes
/// scroll-related state, including scrollbar visibility and current scroll offsets.
/// </summary>
/// <remarks>
/// Implementations bridge the <see cref="Viewer"/> with a concrete scrolling surface
/// (for example, a control that renders and scrolls the viewer's child content).
/// Offsets are expressed in device-independent pixels (DIPs).
/// </remarks>
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
    /// <remarks>
    /// Visibility typically depends on whether the vertical content extent exceeds the viewport height
    /// and on the configured scrollbar visibility policy of the host.
    /// </remarks>
    bool IsVerticalScrollBarVisible { get; }

    /// <summary>
    /// Gets a value indicating whether the horizontal scroll bar is currently visible.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a horizontal scroll bar is visible; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Visibility typically depends on whether the horizontal content extent exceeds the viewport width
    /// and on the configured scrollbar visibility policy of the host.
    /// </remarks>
    bool IsHorizontalScrollBarVisible { get; }

    /// <summary>
    /// Gets or sets the horizontal scroll offset in device-independent pixels (DIPs).
    /// </summary>
    /// <value>
    /// The horizontal offset where <c>0</c> represents the leftmost position. Implementations should clamp
    /// this value to the valid scrollable range.
    /// </value>
    /// <remarks>
    /// Setting this property should update the viewport and may trigger a re-render or arrange pass
    /// of the child content as appropriate for the host.
    /// </remarks>
    float HorizontalOffset { get; set; }

    /// <summary>
    /// Gets or sets the vertical scroll offset in device-independent pixels (DIPs).
    /// </summary>
    /// <value>
    /// The vertical offset where <c>0</c> represents the topmost position. Implementations should clamp
    /// this value to the valid scrollable range.
    /// </value>
    /// <remarks>
    /// Setting this property should update the viewport and may trigger a re-render or arrange pass
    /// of the child content as appropriate for the host.
    /// </remarks>
    float VerticalOffset { get; set; }
}
