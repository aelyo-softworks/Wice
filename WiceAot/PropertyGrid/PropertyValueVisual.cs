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
    public virtual IEditorCreator CreateDefaultEditorCreator()
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
    public virtual IEditorCreator<T> CreateDefaultEditorCreator()
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
    /// Creates the editor based on <see cref="PropertyGridPropertyOptionsAttribute"/> set on the target property,
    /// or uses default behavior when none is provided.
    /// </summary>
    protected internal virtual void CreateEditor()
    {
        var options = Property.Options ?? new PropertyGridPropertyOptionsAttribute();
        var cae = CreateCreatorAndEditor(options);
        EditorCreator = cae?.Creator;
        AddEditor(cae?.Editor);
    }

    /// <summary>
    /// Creates an instance of <see cref="CreatorAndEditor"/> that contains an editor creator and its associated editor.
    /// </summary>
    /// <param name="options">Optional configuration for the editor creator. If specified, the <see
    /// cref="PropertyGridPropertyOptionsAttribute.EditorCreatorType"/>  property may be used to determine the type of
    /// the editor creator to instantiate.</param>
    /// <returns>A <see cref="CreatorAndEditor"/> instance containing the editor creator and the created editor, or <see
    /// langword="null"/>  if no suitable editor creator or editor could be created.</returns>
    public virtual CreatorAndEditor? CreateCreatorAndEditor(PropertyGridPropertyOptionsAttribute? options = null)
    {
#if NETFRAMEWORK
        var creator = Property.Value as IEditorCreator;
#else
        var creator = Property.Value as IEditorCreator<T>;
#endif
        if (creator == null)
        {
            var type = options?.EditorCreatorType;
            if (type != null)
            {
                var editorCreator = Activator.CreateInstance(type);
#if NETFRAMEWORK
                creator = editorCreator as IEditorCreator;
                if (creator == null)
                    throw new WiceException("0024: type '" + type.FullName + "' doesn't implement the " + nameof(IEditorCreator) + " interface.");
#else
                creator = editorCreator as IEditorCreator<T>;
                if (creator == null)
                    throw new WiceException("0024: type '" + type.FullName + "' doesn't implement the " + nameof(IEditorCreator<T>) + " interface.");
#endif
            }
        }

        // fall back to default editor
        creator ??= CreateDefaultEditorCreator();
        if (creator != null)
        {
            var editor = creator.CreateEditor(this);
            if (editor != null)
                return new CreatorAndEditor(creator, editor);
        }

        return null;
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

#if NETFRAMEWORK
    public class CreatorAndEditor(IEditorCreator? creator, object? editor)
    {
        public IEditorCreator? Creator { get; } = creator;
        public object? Editor { get; } = editor;
    }
#else
    /// <summary>
    /// Represents a pair consisting of an editor creator and an associated editor instance.
    /// </summary>
    /// <param name="creator"></param>
    /// <param name="editor"></param>
    public class CreatorAndEditor(IEditorCreator<T>? creator, object? editor)
    {
        /// <summary>
        /// Gets the creator responsible for generating editor instances of type <typeparamref name="T"/>.
        /// </summary>
        public IEditorCreator<T>? Creator { get; } = creator;

        /// <summary>
        /// Gets the editor associated with the current context.
        /// </summary>
        public object? Editor { get; } = editor;
    }
#endif
}
