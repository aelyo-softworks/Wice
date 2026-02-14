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
public class BaseSliderEditorCreator<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Tn>(Orientation orientation) : IEditorCreator<T> where Tn : INumber<Tn>, IMinMaxValue<Tn>
{
    private readonly BaseSlider<Tn> _slider = new(orientation);

    /// <inheritdoc/>
    public object? CreateEditor(PropertyValueVisual<T> value) => _slider;

    /// <inheritdoc/>
    public object? UpdateEditor(PropertyValueVisual<T> value, object? editor) => editor;
}
