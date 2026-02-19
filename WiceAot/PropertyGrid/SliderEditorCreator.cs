using System.ComponentModel.DataAnnotations;

namespace Wice.PropertyGrid;

/// <summary>
/// Provides a base class for creating slider-based property editors that support a specified numeric type and
/// orientation.
/// </summary>
/// <typeparam name="T">The type of the property value that the editor will handle.</typeparam>
/// <typeparam name="Tn">The numeric type that defines the range and behavior of the slider. Must implement INumber and IMinMaxValue.</typeparam>
/// <remarks>
/// Initializes a new instance of the BaseSliderEditorCreator class with the specified slider orientation.
/// </remarks>
/// <param name="orientation">The orientation of the slider, which determines whether the slider is laid out horizontally or vertically.</param>
public class SliderEditorCreator<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Tn>(Orientation orientation) : IEditorCreator<T> where Tn : INumber<Tn>, IMinMaxValue<Tn>
{
    private Slider<Tn>? _slider;

    /// <summary>
    /// Creates a new instance of the Slider class with the specified orientation.
    /// </summary>
    /// <returns>A Slider instance configured with the current orientation.</returns>
    protected virtual Slider<Tn> CreateSlider() => new() { Orientation = orientation };

    /// <inheritdoc/>
    public virtual object? CreateEditor(PropertyValueVisual<T> value)
    {
        _slider ??= CreateSlider();
        if (_slider == null)
            throw new InvalidOperationException();

        var range = value.Property.Info.GetCustomAttribute<RangeAttribute>();
        if (range != null)
        {
            if (Conversions.TryChangeType<Tn>(range.Minimum, out var min))
            {
                _slider.MinValue = min!;
            }

            if (Conversions.TryChangeType<Tn>(range.Maximum, out var max))
            {
                _slider.MaxValue = max!;
            }
        }

        var allowed = value.Property.Info.GetCustomAttribute<AllowedValuesAttribute>();
        if (allowed != null && allowed.Values != null && allowed.Values.Length > 0)
        {
            var list = new List<Tn>();
            foreach (var allowedValue in allowed.Values)
            {
                if (Conversions.TryChangeType<Tn>(allowedValue, out var allowedTn))
                {
                    list.Add(allowedTn!);
                }
            }

            _slider.TicksSteps = [.. list];
        }

        var att = value.Property.Info.GetCustomAttribute<SliderEditorAttribute<Tn>>();
        if (att != null)
        {
            if (att.MinValue != null && Conversions.TryChangeType<Tn>(att.MinValue, out var min))
            {
                _slider.MinValue = min!;
            }

            if (att.MaxValue != null && Conversions.TryChangeType<Tn>(att.MaxValue, out var max))
            {
                _slider.MaxValue = max!;
            }

            if (att.TicksSteps != null && att.TicksSteps.Length > 0)
            {
                var list = new List<Tn>();
                foreach (var step in att.TicksSteps)
                {
                    if (Conversions.TryChangeType<Tn>(step, out var ticksStepTn))
                    {
                        list.Add(ticksStepTn!);
                    }
                }
                _slider.TicksSteps = [.. list];
            }

            if (att.TicksStep != null && Conversions.TryChangeType<Tn>(att.TicksStep, out var ticksStep))
            {
                _slider.TicksStep = ticksStep!;
            }

            if (att.KeyboardStep != null && Conversions.TryChangeType<Tn>(att.KeyboardStep, out var keyboardStep))
            {
                _slider.KeyboardStep = keyboardStep!;
            }

            _slider.SnapToTicks = att.SnapToTicks;
            _slider.TicksOptions = att.TicksOptions;
            _slider.Orientation = att.Orientation;
            _slider.TextOrientation = att.TextOrientation;
        }

        if (Conversions.TryChangeType<Tn>(value.Property.Value, out var v))
        {
            _slider.Value = v!;
        }
        return _slider;
    }

    /// <inheritdoc/>
    public virtual object? UpdateEditor(PropertyValueVisual<T> value, object? editor) => editor;
}
