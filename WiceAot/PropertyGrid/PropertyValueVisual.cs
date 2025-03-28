namespace Wice.PropertyGrid;

public partial class PropertyValueVisual<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : Border
{
    public PropertyValueVisual(PropertyGridProperty<T> property)
    {
        ArgumentNullException.ThrowIfNull(property);

        Property = property;

#if DEBUG
        Name = property.DisplayName + "='" + property.TextValue + "'";
#endif
    }

    [Browsable(false)]
    public new PropertyGrid<T>? Parent => (PropertyGrid<T>?)base.Parent;

    [Browsable(false)]
    public PropertyGridProperty<T> Property { get; }

    [Browsable(false)]
    public object? Editor { get; private set; }

    [Browsable(false)]
    public IEditorCreator<T>? EditorCreator { get; private set; }

    public override string ToString() => base.ToString() + " | [" + Property.DisplayName + "='" + Property.TextValue + " ']";

    public virtual void UpdateEditor()
    {
        Property.UpdateValueFromSource();

        var creator = EditorCreator;
        if (creator != null)
        {
            if (Editor is EditorHost host)
            {
                host.Header.Text.Text = Property.TextValue ?? string.Empty;
            }

            var newEditor = creator.UpdateEditor(this, Editor);
            AddEditor(newEditor);
        }
    }

    public virtual IEditorCreator<T> CreateEditorCreator()
    {
        if (typeof(bool).IsAssignableFrom(Property.Type))
            return new BooleanEditorCreator<T>();

        if (Property.Type?.IsEnum == true && Property.IsReadWrite)
            return new EnumEditorCreator<T>();

        return new DefaultEditorCreator<T>();
    }

    public virtual object? CreateDefaultEditor()
    {
        EditorCreator = CreateEditorCreator();
        if (EditorCreator == null)
            return null;

        return EditorCreator.CreateEditor(this);
    }

    public virtual void CreateEditor()
    {
        var options = Property.Options ?? new PropertyGridPropertyOptionsAttribute();
        var editor = options.CreateEditor(this);
        AddEditor(editor);
    }

    protected virtual void AddEditor(object? editor)
    {
        if (editor == Editor)
            return;

        if (Editor != null)
        {
            if (Editor is Visual editorVisual)
            {
                Children.Remove(editorVisual);
            }
        }

        if (editor is IValueable valuable)
        {
            valuable.CanChangeValue = Property.IsReadWrite;
            valuable.ValueChanged += (s, e) =>
            {
                Property.Value = e.Value;
            };
        }

        if (editor is Visual visual)
        {
            Children.Add(visual);

            if (Property.IsReadOnly)
            {
                visual.Opacity *= Application.CurrentTheme.DisabledOpacityRatio;
            }
        }

        Editor = editor;
    }
}
