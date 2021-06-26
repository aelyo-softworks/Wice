using System;

namespace Wice.PropertyGrid
{
    public class PasswordEditorCreator : IEditorCreator
    {
        public object CreateEditor(PropertyValueVisual value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

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

        public object UpdateEditor(PropertyValueVisual value, object editor)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return editor;
        }
    }
}
