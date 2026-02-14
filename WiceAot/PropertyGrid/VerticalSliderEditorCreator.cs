namespace Wice.PropertyGrid;

/// <summary>
/// Provides a slider editor that enables users to select a value within a specified range using a vertical
/// orientation.
/// </summary>
/// <typeparam name="T">The type of the value represented by the slider. Must support public properties.</typeparam>
/// <typeparam name="Tn">The numeric type that defines the slider's minimum and maximum values. Must implement both INumber and
/// IMinMaxValue.</typeparam>
public class VerticalSliderEditorCreator<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Tn> : BaseSliderEditorCreator<T, Tn> where Tn : INumber<Tn>, IMinMaxValue<Tn>
{
    /// <summary>
    /// Initializes a new instance of the VerticalSliderEditorCreator class with a vertical orientation.
    /// </summary>
    public VerticalSliderEditorCreator()
        : base(Orientation.Vertical)
    {
    }
}
