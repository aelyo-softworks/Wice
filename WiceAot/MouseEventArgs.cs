namespace Wice;

/// <summary>
/// Provides event data for mouse input, including window-relative coordinates,
/// modifier/button state, and (optionally) a source pointer event for richer context.
/// </summary>
/// <remarks>
/// - Coordinates (<see cref="X"/>/<see cref="Y"/> and <see cref="Point"/>) are in window space.
///   Use <see cref="GetPosition(Visual)"/> to convert to a visual-relative point.
/// - <see cref="VisualsStack"/> exposes the visuals intersected during hit-testing, ordered by z-order (top-most first).
///   When this mouse event is backed by a pointer event that carries position
///   (i.e., <see cref="PointerPositionEventArgs"/>), it delegates to that event's stack.
/// - <see cref="Keys"/> contains the modifier/button state flags captured for the event.
/// </remarks>
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
    /// <remarks>
    /// This is <see langword="null"/> when Mouse-In-Pointer integration is disabled.
    /// When present and the instance is a <see cref="PointerPositionEventArgs"/>,
    /// <see cref="VisualsStack"/> data is delegated to the pointer event.
    /// </remarks>
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
    /// <remarks>
    /// If <see cref="SourcePointerEvent"/> is a <see cref="PointerPositionEventArgs"/>,
    /// the value is taken from that event to preserve the original hit-test ordering.
    /// Otherwise, an internal stack populated by the input pipeline is returned.
    /// </remarks>
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visual"/> is <see langword="null"/>.</exception>
    /// <seealso cref="Visual.GetRelativePosition(int, int)"/>
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visual"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// Uses <see cref="Visual.RenderSize"/> and a point relative to the visual. This does not account for
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

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string containing X, Y, and VK values.</returns>
    public override string ToString() => "X=" + X + ",Y=" + Y + ",VK=" + Keys;
}
