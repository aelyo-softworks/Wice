namespace Wice.PropertyGrid;

/// Visual hosting the editor for a single <see cref="PropertyGridProperty{T}"/> value.
/// </summary>
/// <typeparam name="T">
/// The selected object type for the owning <see cref="PropertyGrid{T}"/>. The attribute requires public properties for trimming.
/// </typeparam>
/// <remarks>
/// Responsibilities:
/// - Creates an editor via <see cref="PropertyGridPropertyOptionsAttribute"/> or a chosen <see cref="IEditorCreator{T}"/>.
/// - Keeps the displayed editor synchronized with the underlying <see cref="Property"/> value (<see cref="UpdateEditor"/>).
/// - Bridges editor value changes back into the property when the editor implements <see cref="IValueable"/>.
/// - Applies a disabled visual state (opacity) for read-only properties.
/// </remarks>
public partial class PropertyValueVisual<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : Border
{
    /// <summary>
    /// Initializes a new instance bound to the given property.
    /// </summary>
    /// <param name="property">The property metadata/value wrapper to display and edit.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is null.</exception>
    public PropertyValueVisual(PropertyGridProperty<T> property)
    {
        ArgumentNullException.ThrowIfNull(property);

        Property = property;

#if DEBUG
        // Useful in diagnostics and the Visual Tree viewer.
        Name = property.DisplayName + "='" + property.TextValue + "'";
#endif
    }

    /// <summary>
    /// Gets the owning typed <see cref="PropertyGrid{T}"/> parent.
    /// </summary>
    /// <remarks>
    /// Shadows <see cref="Visual.Parent"/> to expose a strongly-typed grid parent, when available.
    /// </remarks>
    [Browsable(false)]
    public new PropertyGrid<T>? Parent => (PropertyGrid<T>?)base.Parent;

    /// <summary>
    /// Gets the property represented by this visual.
    /// </summary>
    [Browsable(false)]
    public PropertyGridProperty<T> Property { get; }

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
    public IEditorCreator<T>? EditorCreator { get; set; }

    /// <inheritdoc/>
    public override string ToString() => base.ToString() + " | [" + Property.DisplayName + "='" + Property.TextValue + " ']";

    /// <summary>
    /// Refreshes the editor UI from the current property value and allows the creator to update/replace the editor.
    /// </summary>
    /// <remarks>
    /// - Pulls the latest source value into <see cref="Property"/> via <see cref="PropertyGridProperty{T}.UpdateValueFromSource"/>.<br/>
    /// - Updates the header text if the editor is hosted inside an <see cref="EditorHost"/>.<br/>
    /// - Invokes <see cref="IEditorCreator{T}.UpdateEditor"/> and applies the result through <see cref="AddEditor(object?)"/>.
    /// </remarks>
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
    public virtual IEditorCreator<T> CreateEditorCreator()
    {
        if (typeof(bool).IsAssignableFrom(Property.Type))
            return new BooleanEditorCreator<T>();

        if (Property.Type?.IsEnum == true && Property.IsReadWrite)
            return new EnumEditorCreator<T>();

        return new DefaultEditorCreator<T>();
    }

    /// <summary>
    /// Creates the default editor using the selected <see cref="EditorCreator"/>.
    /// </summary>
    /// <returns>The created editor instance or <see langword="null"/>.</returns>
    /// <remarks>
    /// This sets <see cref="EditorCreator"/> to the result of <see cref="CreateEditorCreator"/> before creating the editor.
    /// </remarks>
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
    /// <remarks>
    /// The created editor is inserted via <see cref="AddEditor(object?)"/>.
    /// </remarks>
    public virtual void CreateEditor()
    {
        var options = Property.Options ?? new PropertyGridPropertyOptionsAttribute();
        var editor = options.CreateEditor(this);
        AddEditor(editor);
    }

    /// <summary>
    /// Inserts the given editor into the visual tree and wires its value change notifications.
    /// </summary>
    /// <param name="editor">The editor instance to attach; can be <see cref="Visual"/> and/or <see cref="IValueable"/>.</param>
    /// <remarks>
    /// Behavior:
    /// - If the editor did not change, no work is performed.<br/>
    /// - Removes the previous editor <see cref="Visual"/> (when applicable).<br/>
    /// - If the new editor implements <see cref="IValueable"/>, sets <c>CanChangeValue</c> and forwards <c>ValueChanged</c> to <see cref="Property"/> Value.<br/>
    /// - Adds the editor <see cref="Visual"/> to <see cref="Visual.Children"/> and dims opacity when <see cref="Property"/> is read-only.
    /// </remarks>
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
