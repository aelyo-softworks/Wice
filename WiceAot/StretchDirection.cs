namespace Wice;

/// <summary>
/// Specifies how content is allowed to scale to fit the available space.
/// </summary>
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
