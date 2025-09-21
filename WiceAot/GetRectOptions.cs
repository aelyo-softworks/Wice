namespace Wice;

/// <summary>
/// Bitwise flags that control how a rectangle should be sized relative to its parent.
/// Combine values using bitwise OR to achieve the desired behavior.
/// </summary>
[Flags]
public enum GetRectOptions
{
    /// <summary>
    /// No special sizing behavior; use the content's natural size.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Stretch the rectangle's width to match the parent's available width.
    /// </summary>
    StretchWidthToParent = 0x1,

    /// <summary>
    /// Stretch the rectangle's height to match the parent's available height.
    /// </summary>
    StretchHeightToParent = 0x2,

    /// <summary>
    /// Preserve aspect ratio when stretching; the non-stretched dimension is derived from the stretched one.
    /// </summary>
    KeepProportions = 0x4,

    /// <summary>
    /// Shortcut for stretching both width and height to the parent.
    /// </summary>
    Default = StretchWidthToParent | StretchHeightToParent,
}
