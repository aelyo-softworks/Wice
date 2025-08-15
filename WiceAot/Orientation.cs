namespace Wice;

/// <summary>
/// Specifies the layout direction for arranging UI elements or content.
/// </summary>
/// <remarks>
/// - Horizontal: Arrange along the X axis (typically left-to-right in LTR locales).
/// - Vertical: Arrange along the Y axis (top-to-bottom).
/// </remarks>
public enum Orientation
{
    /// <summary>
    /// Content flows along the horizontal (X) axis.
    /// </summary>
    /// <remarks>
    /// Typically left-to-right in LTR locales and right-to-left in RTL locales.
    /// </remarks>
    Horizontal,

    /// <summary>
    /// Content flows along the vertical (Y) axis, top-to-bottom.
    /// </summary>
    Vertical,
}
