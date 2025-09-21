namespace Wice;

/// <summary>
/// Specifies the scope of UI invalidation for layout and rendering operations.
/// </summary>
public enum InvalidateMode
{
    /// <summary>
    /// No invalidation is requested.
    /// </summary>
    None,

    /// <summary>
    /// Invalidates only the rendering/visual pass.
    /// No layout (arrange/measure) is performed.
    /// </summary>
    Render,

    /// <summary>
    /// Invalidates the arrange (and render) passes.
    /// Element sizes are assumed stable; positions and visuals may change.
    /// </summary>
    Arrange,

    /// <summary>
    /// Invalidates the full layout, starting with the measure pass
    /// (which implies arrange and render).
    /// </summary>
    Measure,
}
