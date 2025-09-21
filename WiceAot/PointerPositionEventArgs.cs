namespace Wice;

/// <summary>
/// Base event data for pointer events that carry an absolute window-space position.
/// </summary>
/// <param name="pointerId">The system-assigned identifier of the pointer that generated the event.</param>
/// <param name="x">The X coordinate of the pointer in window coordinates.</param>
/// <param name="y">The Y coordinate of the pointer in window coordinates.</param>
public abstract class PointerPositionEventArgs(uint pointerId, int x, int y) : PointerEventArgs(pointerId)
{
    /// <summary>
    /// Internal stack of visuals intersected during hit-testing for this event (top-most first).
    /// </summary>
    internal readonly List<Visual> _visualsStack = [];

    /// <summary>
    /// Gets the X coordinate of the pointer in absolute window coordinates.
    /// </summary>
    public int X { get; } = x;

    /// <summary>
    /// Gets the Y coordinate of the pointer in absolute window coordinates.
    /// </summary>
    public int Y { get; } = y;

    /// <summary>
    /// Gets the visuals that were intersected during hit-testing for this event.
    /// </summary>
    public IReadOnlyList<Visual> VisualsStack => _visualsStack;

    /// <summary>
    /// Converts the absolute window position to coordinates relative to the specified <paramref name="visual"/>.
    /// </summary>
    /// <param name="visual">The visual to which the position should be transformed.</param>
    /// <returns>The pointer position relative to <paramref name="visual"/>.</returns>
    public POINT GetPosition(Visual visual)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        return visual.GetRelativePosition(X, Y);
    }

    /// <summary>
    /// Determines whether the pointer position lies within the render bounds of <paramref name="visual"/>.
    /// </summary>
    /// <param name="visual">The visual to test against.</param>
    /// <returns>true if the pointer is within the visual's render size; otherwise, false.</returns>
    public bool Hits(Visual visual)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        var size = visual.RenderSize;
        if (size.IsInvalid)
            return false;

        return new D2D_RECT_F(size).Contains(GetPosition(visual));
    }

    /// <inheritdoc/>
    public override string ToString() => base.ToString() + ",X=" + X + ",Y=" + Y;
}
