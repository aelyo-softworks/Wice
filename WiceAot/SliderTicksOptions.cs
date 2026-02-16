namespace Wice;

/// <summary>
/// Specifies options for displaying tick marks on a slider control.
/// </summary>
[Flags]
public enum SliderTicksOptions
{
    /// <summary>
    /// Represents the default value indicating that no options are specified.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Specifies that tick marks are displayed on the slider control.
    /// </summary>
    ShowTicks = 0x1,

    /// <summary>
    /// Specifies that tick values are displayed on the axis.
    /// </summary>
    ShowTickValues = 0x2,
}
