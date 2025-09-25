namespace Wice.PropertyGrid;

/// <summary>
/// Represents a visual element for displaying and editing a property within a <see cref="PropertyGrid{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the object containing the property being displayed and edited.</typeparam>
#if NETFRAMEWORK
public partial class PropertyValueVisual : Border
#else
public partial class PropertyValueVisual<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : Border
#endif
{
    /// <summary>
    /// Initializes a new instance bound to the given property.
    /// </summary>
    /// <param name="property">The property metadata/value wrapper to display and edit.</param>
#if NETFRAMEWORK
    public PropertyValueVisual(PropertyGridProperty property)
#else
    public PropertyValueVisual(PropertyGridProperty<T> property)
#endif
    {
        ExceptionExtensions.ThrowIfNull(property, nameof(property));

        Property = property;

#if DEBUG
        // Useful in diagnostics and the Visual Tree viewer.
        Name = property.DisplayName + "='" + property.TextValue + "'";
#endif
    }

    /// <summary>
    /// Gets the owning typed <see cref="PropertyGrid{T}"/> parent.
    /// </summary>
    [Browsable(false)]
#if NETFRAMEWORK
    public new PropertyGrid? Parent => (PropertyGrid?)base.Parent;
#else
    public new PropertyGrid<T>? Parent => (PropertyGrid<T>?)base.Parent;
#endif

    /// <summary>
    /// Gets the property represented by this visual.
    /// </summary>
    [Browsable(false)]
#if NETFRAMEWORK
    public PropertyGridProperty Property { get; }
#else
    public PropertyGridProperty<T> Property { get; }
#endif

    /// <summary>
    /// Gets the current editor instance (often a <see cref="Visual"/>, optionally an <see cref="EditorHost"/>),
    /// or <see langword="null"/> when not created yet.
    /// </summary>
    [Browsable(false)]
    public object? Editor { get; private set; }

    /// <summary>
    /// Gets or sets the creator responsible for building/updating the editor instance.
    /// </summary>
    [Browsable(false)]
#if NETFRAMEWORK
    public IEditorCreator? EditorCreator { get; set; }
#else
    public IEditorCreator<T>? EditorCreator { get; set; }
#endif

    /// <inheritdoc/>
    public override string ToString() => base.ToString() + " | [" + Property.DisplayName + "='" + Property.TextValue + " ']";

    /// <summary>
    /// Refreshes the editor UI from the current property value and allows the creator to update/replace the editor.
    /// </summary>
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

    /// <summary>
    /// Chooses an <see cref="IEditorCreator{T}"/> for the property type and state.
    /// </summary>
    /// <returns>
    /// A boolean, enum, or default editor creator depending on <see cref="Property"/>.
    /// </returns>
#if NETFRAMEWORK
    public virtual IEditorCreator CreateEditorCreator()
    {
        if (typeof(bool).IsAssignableFrom(Property.Type))
            return new BooleanEditorCreator();

        if (typeof(bool?).IsAssignableFrom(Property.Type))
            return new NullableBooleanEditorCreator();

        if (Property.Type.IsEnum && Property.IsReadWrite)
            return new EnumEditorCreator();

        return new DefaultEditorCreator();
    }
#else
    public virtual IEditorCreator<T> CreateEditorCreator()
    {
        if (typeof(bool).IsAssignableFrom(Property.Type))
            return new BooleanEditorCreator<T>();

        if (typeof(bool?).IsAssignableFrom(Property.Type))
            return new NullableBooleanEditorCreator<T>();

        if (Property.Type?.IsEnum == true && Property.IsReadWrite)
            return new EnumEditorCreator<T>();

        return new DefaultEditorCreator<T>();
    }
#endif

    /// <summary>
    /// Creates the default editor using the selected <see cref="EditorCreator"/>.
    /// </summary>
    /// <returns>The created editor instance or <see langword="null"/>.</returns>
    public virtual object? CreateDefaultEditor()
    {
        EditorCreator = CreateEditorCreator();
        if (EditorCreator == null)
            return null;

        return EditorCreator.CreateEditor(this);
    }

    /// <summary>
    /// Creates the editor based on <see cref="PropertyGridPropertyOptionsAttribute"/> set on the target property,
    /// or uses default behavior when none is provided.
    /// </summary>
    public virtual void CreateEditor()
    {
        var options = Property.Options ?? new PropertyGridPropertyOptionsAttribute();
        var editorAndCreator = options.CreateEditor(this);
#if NETFRAMEWORK
        EditorCreator = editorAndCreator.Item2;
        AddEditor(editorAndCreator.Item1);
#else
        EditorCreator = editorAndCreator.EditorCreator;
        AddEditor(editorAndCreator.Editor);
#endif
    }

    /// <summary>
    /// Inserts the given editor into the visual tree and wires its value change notifications.
    /// </summary>
    /// <param name="editor">The editor instance to attach; can be <see cref="Visual"/> and/or <see cref="IValueable"/>.</param>
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
                visual.Opacity *= GetWindowTheme().DisabledOpacityRatio;
            }
        }

        Editor = editor;
    }
}
