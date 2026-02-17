namespace Wice.PropertyGrid;

/// <summary>
/// Provides a slider editor that enables users to select a value within a specified range using a horizontal
/// orientation.
/// </summary>
/// <typeparam name="T">The type of the value represented by the slider. Must support public properties.</typeparam>
/// <typeparam name="Tn">The numeric type that defines the slider's minimum and maximum values. Must implement both INumber and
/// IMinMaxValue.</typeparam>
public class HorizontalSliderEditorCreator<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Tn> : SliderEditorCreator<T, Tn> where Tn : INumber<Tn>, IMinMaxValue<Tn>
{
    /// <summary>
    /// Initializes a new instance of the HorizontalSliderEditorCreator class with a horizontal orientation.
    /// </summary>
    public HorizontalSliderEditorCreator()
        : base(Orientation.Horizontal)
    {
    }
}
