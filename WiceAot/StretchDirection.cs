namespace Wice;

/// <summary>
/// Specifies how content is allowed to scale to fit the available space.
/// </summary>
/// <remarks>
/// Use this to constrain resizing behavior relative to the content's natural (desired) size:
/// - UpOnly: content may grow but will not shrink.
/// - DownOnly: content may shrink but will not grow.
/// - Both: content may grow or shrink.
/// </remarks>
public enum StretchDirection
{
    /// <summary>
    /// Content may be scaled up beyond its natural (desired) size, but will not be scaled down.
    /// </summary>
    UpOnly,

    /// <summary>
    /// Content may be scaled down below its natural (desired) size, but will not be scaled up.
    /// </summary>
    DownOnly,

    /// <summary>
    /// Content may be scaled both up and down relative to its natural (desired) size.
    /// </summary>
    Both
}
