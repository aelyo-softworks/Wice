namespace Wice.PropertyGrid;

public class BooleanEditorCreator : IEditorCreator
{
    public object CreateEditor(PropertyValueVisual value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var toggle = new ToggleSwitch();
        toggle.HorizontalAlignment = Alignment.Near;

        if (value.Property.TryGetTargetValue(out bool targetValue))
        {
            toggle.Value = targetValue;
        }
        return toggle;
    }

    public object UpdateEditor(PropertyValueVisual value, object editor)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        return editor;
    }
}
