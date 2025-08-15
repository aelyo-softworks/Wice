namespace Wice;

/// <summary>
/// Defines invalidation modes for visual property changes.
/// Combine flags to indicate which layout/rendering operations should be re-evaluated,
/// and whether invalidation should also propagate to the parent visual.
/// </summary>
/// <remarks>
/// - Render: Repaint is required.
/// - Arrange: A new arrange pass is required.
/// - Measure: A new measure pass is required.
/// - Parent*: Propagates the corresponding invalidation to the parent visual.
/// Flags can be combined using bitwise operations.
/// </remarks>
[Flags]
public enum VisualPropertyInvalidateModes
{
    /// <summary>
    /// No invalidation is required.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// The visual must be redrawn (re-rendered).
    /// </summary>
    Render = 0x1,

    /// <summary>
    /// The visual requires a new arrange pass.
    /// </summary>
    Arrange = 0x2,

    /// <summary>
    /// The visual requires a new measure pass.
    /// </summary>
    Measure = 0x4,

    /// <summary>
    /// Propagate a render invalidation to the parent visual.
    /// </summary>
    ParentRender = 0x8,

    /// <summary>
    /// Propagate an arrange invalidation to the parent visual.
    /// </summary>
    ParentArrange = 0x10,

    /// <summary>
    /// Propagate a measure invalidation to the parent visual.
    /// </summary>
    ParentMeasure = 0x20,
}
