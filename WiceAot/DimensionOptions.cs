namespace Wice;

/// <summary>
/// Specifies which dimensions of an element are automatically sized.
/// This enumeration supports bitwise combination of its member values.
/// </summary>
[Flags]
public enum DimensionOptions
{
    /// <summary>
    /// No automatic sizing; both width and height are controlled manually.
    /// </summary>
    Manual = 0x0,

    /// <summary>
    /// Width is automatically sized; height is controlled manually.
    /// </summary>
    Width = 0x1,

    /// <summary>
    /// Height is automatically sized; width is controlled manually.
    /// </summary>
    Height = 0x2,

    /// <summary>
    /// Both width and height are automatically sized; equivalent to <see cref="Width"/> | <see cref="Height"/>.
    /// </summary>
    WidthAndHeight = Width | Height,
}
