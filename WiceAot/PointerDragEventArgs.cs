namespace Wice;

/// <summary>
/// Provides event data for a pointer drag gesture, combining the latest pointer update
/// information with the current <see cref="DragState"/> describing the gesture progression.
/// </summary>
/// <remarks>
/// This type is raised by <see cref="Visual.OnPointerDrag(object?, PointerDragEventArgs)"/> during
/// pointer-driven drag operations. The base <see cref="PointerUpdateEventArgs"/> exposes the pointer
/// identifier, window-relative coordinates, and message flags at the time of the event, while
/// <see cref="State"/> carries gesture state such as the drag start position and current deltas.
/// <para>
/// Coordinates reported by this event are window-relative (composition space), matching
/// <see cref="PointerPositionEventArgs"/> semantics.
/// </para>
/// </remarks>
/// <seealso cref="PointerUpdateEventArgs"/>
/// <seealso cref="PointerPositionEventArgs"/>
/// <seealso cref="DragState"/>
/// <seealso cref="Visual.OnPointerDrag(object?, PointerDragEventArgs)"/>
public class PointerDragEventArgs : PointerUpdateEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PointerDragEventArgs"/> class using values from
    /// a source <see cref="PointerUpdateEventArgs"/> and the current <see cref="DragState"/>.
    /// </summary>
    /// <param name="e">The source pointer update event providing pointer id, position, and flags.</param>
    /// <param name="state">The current drag state for the active gesture.</param>
    /// <exception cref="ArgumentNullException"><paramref name="e"/> is null.</exception>
    /// <remarks>
    /// The constructor copies the primitive values from <paramref name="e"/> into the base
    /// <see cref="PointerUpdateEventArgs"/> and stores <paramref name="state"/> for access via
    /// <see cref="State"/>. A null check is enforced via <c>ExceptionExtensions.ThrowIfNull</c>.
    /// </remarks>
    public PointerDragEventArgs(PointerUpdateEventArgs e, DragState state)
        : base((e?.PointerId).GetValueOrDefault(), (e?.X).GetValueOrDefault(), (e?.Y).GetValueOrDefault(), (e?.Flags).GetValueOrDefault())
    {
        ExceptionExtensions.ThrowIfNull(e, nameof(e));
        State = state;
    }

    /// <summary>
    /// Gets the current drag state associated with this pointer drag event.
    /// </summary>
    /// <remarks>
    /// The drag state contains the drag start coordinates and current deltas. It can be used to implement
    /// behaviors such as moving visuals, resizing, or drawing selection rectangles as the pointer moves.
    /// </remarks>
    /// <example>
    /// For example, inside a visual's drag handler:
    /// <code>
    /// protected override void OnPointerDrag(object? sender, PointerDragEventArgs e)
    /// {
    ///     // Apply translation based on current drag delta
    ///     var dx = e.State.DeltaX;
    ///     var dy = e.State.DeltaY;
    ///     RenderOffset += new System.Numerics.Vector3(dx, dy, 0);
    /// }
    /// </code>
    /// </example>
    public DragState State { get; }
}
