namespace Wice.PropertyGrid;

public class BooleanEditorCreator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IEditorCreator<T>
{
    public object? CreateEditor(PropertyValueVisual<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var toggle = new ToggleSwitch { HorizontalAlignment = Alignment.Near };
        if (value.Property.TryGetTargetValue(out bool targetValue))
        {
            toggle.Value = targetValue;
        }
        return toggle;
    }

    public object? UpdateEditor(PropertyValueVisual<T> value, object? editor)
    {
        ArgumentNullException.ThrowIfNull(value);
        return editor;
    }
}
