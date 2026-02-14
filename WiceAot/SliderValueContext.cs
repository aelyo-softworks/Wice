namespace Wice;

/// <summary>
/// Specifies the context in which a slider value is interpreted, such as representing a maximum, minimum, or tooltip
/// value.
/// </summary>
public enum SliderValueContext
{
    /// <summary>
    /// Unspecified context, where the slider value does not have a specific meaning.
    /// </summary>
    Unspecified,

    /// <summary>
    /// The slider is working with it MaxValue.
    /// </summary>
    MaxValue,

    /// <summary>
    /// The slider is working with it MinValue.
    /// </summary>
    MinValue,

    /// <summary>
    /// The slider is working with a value displayed in the tooltip.
    /// </summary>
    ToolTip,
}
