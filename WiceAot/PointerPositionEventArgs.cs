namespace Wice;

/// <summary>
/// Base event data for pointer events that carry an absolute window-space position.
/// </summary>
/// <param name="pointerId">The system-assigned identifier of the pointer that generated the event.</param>
/// <param name="x">The X coordinate of the pointer in window coordinates.</param>
/// <param name="y">The Y coordinate of the pointer in window coordinates.</param>
/// <remarks>
/// Use <see cref="GetPosition(Visual)"/> to convert the stored window-space coordinates to a visual-relative point,
/// and <see cref="Hits(Visual)"/> for a fast hit-test against a visual's render size.
/// </remarks>
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
    /// <remarks>
    /// The collection is ordered by z-order (top-most first) and is populated by the window input pipeline.
    /// </remarks>
    public IReadOnlyList<Visual> VisualsStack => _visualsStack;

    /// <summary>
    /// Converts the absolute window position to coordinates relative to the specified <paramref name="visual"/>.
    /// </summary>
    /// <param name="visual">The visual to which the position should be transformed.</param>
    /// <returns>The pointer position relative to <paramref name="visual"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visual"/> is null.</exception>
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visual"/> is null.</exception>
    /// <remarks>
    /// This uses <see cref="Visual.RenderSize"/> and a point relative to the visual; it does not account for
    /// complex composition transforms beyond what <see cref="GetPosition(Visual)"/> provides.
    /// </remarks>
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
