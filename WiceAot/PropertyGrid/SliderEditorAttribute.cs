namespace Wice.PropertyGrid;

/// <summary>
/// Specifies options when using a slider editor on a property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SliderEditorAttribute<T> : Attribute where T : INumber<T>, IMinMaxValue<T>
{
    /// <summary>
    /// Gets or sets the minimum allowable value for the property.
    /// </summary>
    public object? MinValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowable value for the property.
    /// </summary>
    public object? MaxValue { get; set; }

    /// <summary>
    /// Gets or sets the keyboard step value of the visual.
    /// </summary>
    public object? KeyboardStep { get; set; }

    /// <summary>
    /// Gets or sets the step value of the ticks.
    /// </summary>
    public object? TicksStep { get; set; }

    /// <summary>
    /// Gets or sets the array of steps for the ticks visual.
    /// </summary>
    public object[]? TicksSteps { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the visual aligns its value to the nearest tick mark on the axis.
    /// </summary>
    public bool SnapToTicks { get; set; }

    /// <summary>
    /// Gets or sets the ticks options of the visual.
    /// </summary>
    public SliderTicksOptions TicksOptions { get; set; } = SliderTicksOptions.ShowTickValues;

    /// <summary>
    /// Gets or sets the slider orientation.
    /// </summary>
    public Orientation Orientation { get; set; }

    /// <summary>
    /// Gets or sets the slider texts orientation.
    /// </summary>
    public Orientation TextOrientation { get; set; }
}
