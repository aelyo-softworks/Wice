namespace Wice.PropertyGrid;

public class PasswordEditorCreator : IEditorCreator
{
    public object? CreateEditor(PropertyValueVisual value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var editor = value.CreateDefaultEditor();
        if (editor is IPasswordCapable pc)
        {
            pc.IsPasswordModeEnabled = true;
            var pw = PropertyGridDynamicPropertyAttribute.GetValueFromProperty<char>(value.Property, "PasswordCharacter");
            if (pw != 0)
            {
                pc.SetPasswordCharacter(pw);
            }
        }
        return editor;
    }

    public object? UpdateEditor(PropertyValueVisual value, object? editor)
    {
        ArgumentNullException.ThrowIfNull(value);
        return editor;
    }
}
