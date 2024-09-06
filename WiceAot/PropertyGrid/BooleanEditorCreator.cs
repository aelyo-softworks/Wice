namespace Wice.PropertyGrid;

public class BooleanEditorCreator : IEditorCreator
{
    public object? CreateEditor(PropertyValueVisual value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var toggle = new ToggleSwitch { HorizontalAlignment = Alignment.Near };
        if (value.Property.TryGetTargetValue(out bool targetValue))
        {
            toggle.Value = targetValue;
        }
        return toggle;
    }

    public object? UpdateEditor(PropertyValueVisual value, object? editor)
    {
        ArgumentNullException.ThrowIfNull(value);
        return editor;
    }
}
