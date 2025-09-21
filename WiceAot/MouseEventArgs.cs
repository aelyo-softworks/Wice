namespace Wice;

/// <summary>
/// Provides event data for mouse input, including window-relative coordinates,
/// modifier/button state, and (optionally) a source pointer event for richer context.
/// </summary>
/// <param name="x">The X coordinate in window-space units.</param>
/// <param name="y">The Y coordinate in window-space units.</param>
/// <param name="vk">The modifier/button flags associated with the event.</param>
public class MouseEventArgs(int x, int y, POINTER_MOD vk)
    : HandledEventArgs
{
    internal readonly List<Visual> _visualsStack = [];

    /// <summary>
    /// Gets the modifier/button state flags captured for this event.
    /// </summary>
    public POINTER_MOD Keys { get; } = vk;

    /// <summary>
    /// Gets the source pointer event used to synthesize this mouse event, when available.
    /// </summary>
    public PointerEventArgs? SourcePointerEvent { get; internal set; } // will be null if EnableMouseInPointer was not called

    /// <summary>
    /// Gets the X coordinate in window space.
    /// </summary>
    public int X { get; } = x;

    /// <summary>
    /// Gets the Y coordinate in window space.
    /// </summary>
    public int Y { get; } = y;

    /// <summary>
    /// Gets the window-space position as a <see cref="POINT"/>.
    /// </summary>
    public POINT Point { get; } = new POINT(x, y);

    /// <summary>
    /// Gets the visuals intersected during hit-testing for this event, ordered by z-order (top-most first).
    /// </summary>
    public IReadOnlyList<Visual> VisualsStack
    {
        get
        {
            if (SourcePointerEvent is not PointerPositionEventArgs evt)
                return _visualsStack;

            return evt._visualsStack;
        }
    }

    /// <summary>
    /// Converts the window-space position to coordinates relative to the specified <paramref name="visual"/>.
    /// </summary>
    /// <param name="visual">The visual to which to convert the position.</param>
    /// <returns>The position relative to <paramref name="visual"/>.</returns>
    public POINT GetPosition(Visual visual)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        return visual.GetRelativePosition(X, Y);
    }

    /// <summary>
    /// Determines whether the window-space position lies within the render bounds of <paramref name="visual"/>.
    /// </summary>
    /// <param name="visual">The visual to test.</param>
    /// <returns><see langword="true"/> if the point is within the visual's render size; otherwise, <see langword="false"/>.</returns>
    public bool Hits(Visual visual)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        var size = visual.RenderSize;
        if (size.IsInvalid)
            return false;

        return new D2D_RECT_F(size).Contains(GetPosition(visual));
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string containing X, Y, and VK values.</returns>
    public override string ToString() => "X=" + X + ",Y=" + Y + ",VK=" + Keys;
}
