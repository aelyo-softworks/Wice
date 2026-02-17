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
    /// Initializes a new instance of the PropertyValueVisual class.
    /// </summary>
    /// <param name="visuals">The visuals associated with the property. This parameter cannot be <see langword="null"/>.</param>
#if NETFRAMEWORK
    public PropertyValueVisual(PropertyVisuals visuals)
#else
    public PropertyValueVisual(PropertyVisuals<T> visuals)
#endif
    {
        ExceptionExtensions.ThrowIfNull(visuals, nameof(visuals));

        Visuals = visuals;

#if DEBUG
        // Useful in diagnostics and the Visual Tree viewer.
        Name = visuals.Property.DisplayName + "='" + visuals.Property.TextValue + "'";
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
    public PropertyGridProperty Property => Visuals.Property;
#else
    public PropertyGridProperty<T> Property => Visuals.Property;
#endif

    /// <summary>
    /// Gets the property visuals.
    /// </summary>
    [Browsable(false)]
#if NETFRAMEWORK
    public PropertyVisuals Visuals { get; }
#else
    public PropertyVisuals<T> Visuals { get; }
#endif

    /// <summary>
    /// Gets the current editor instance (often a <see cref="Visual"/>, optionally an EditorHost),
    /// or <see langword="null"/> when not created yet.
    /// </summary>
    [Browsable(false)]
    public object? Editor { get; private set; }

    /// <summary>
    /// Gets the zero-based row index of the current element within its parent grid.
    /// </summary>
    [Category(CategoryLayout)]
    public int RowIndex => Grid.GetRow(this);

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
#if NETFRAMEWORK
            if (Editor is EditorHost host)
#else
            if (Editor is EditorHost<T> host)
#endif
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
                    throw new WiceException("0024: type '" + type.FullName + "' doesn't implement the " + nameof(IEditorCreator<>) + " interface.");
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

    /// <inheritdoc/>
    protected override void OnKeyDown(object? sender, KeyEventArgs e)
    {
        base.OnKeyDown(sender, e);
        if (HandleTabKeyDown(e))
        {
            e.Handled = true;
        }

        if (!e.Handled)
        {
            HandleClipboardKeys(e);
        }
    }

    /// <summary>
    /// Handles clipboard-related key combinations, such as copying text to the clipboard.
    /// </summary>
    /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data,  including the key pressed and modifier
    /// states.</param>
    protected virtual void HandleClipboardKeys(KeyEventArgs e)
    {
        if (Parent != null && !e.Handled && e.IsDown && e.WithControl && e.Key == VIRTUAL_KEY.VK_C)
        {
            if (e.WithMenu)
            {
                // copy all property grid
                var sb = new StringBuilder();
                foreach (var pv in Parent.PropertyValueVisuals)
                {
                    var name = (pv.Visuals.Text as IValueable)?.Value?.ToString();
                    var value = pv.Children.OfType<IValueable>().FirstOrDefault()?.Value?.ToString();

                    string line;
                    if (string.IsNullOrEmpty(name))
                    {
                        if (string.IsNullOrEmpty(value))
                            continue;

                        line = value;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            line = name;
                        }
                        else
                        {
                            line = name + '\t' + value;
                        }
                    }

                    line = line.Replace('\0', ' ').Trim(); // sanitize nulls
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    sb.AppendLine(line);
                }

                var text = sb.ToString();
                if (!string.IsNullOrEmpty(text))
                {
                    TaskUtilities.RunWithSTAThread(() => Clipboard.SetText(text));
                }
                return;
            }

            var focused = Window?.FocusedVisual;
            var childFocused = focused != null && IsChild(focused);
            if (childFocused)
            {
                var text = Children.OfType<IValueable>().FirstOrDefault()?.Value?.ToString();
                if (e.WithShift) // Shift+CTRL+C copies both name and value
                {
                    var name = (Visuals.Text as IValueable)?.Value?.ToString();
                    if (string.IsNullOrEmpty(text))
                    {
                        text = name;
                    }
                    else if (!string.IsNullOrEmpty(name))
                    {
                        text = name + '\t' + text;
                    }
                }

                if (!string.IsNullOrEmpty(text))
                {
                    TaskUtilities.RunWithSTAThread(() => Clipboard.SetText(text));
                }
            }
        }
    }

    /// <summary>
    /// Handles the behavior when the Tab key is pressed.
    /// </summary>
    /// <param name="e">The <see cref="KeyEventArgs"/> containing information about the key event, such as the key pressed and modifier
    /// states.</param>
    /// <returns><see langword="true"/> if the Tab key press was handled; otherwise, <see langword="false"/>.</returns>
    protected virtual bool HandleTabKeyDown(KeyEventArgs e)
    {
        if (Parent != null && !e.Handled && e.IsDown && !e.WithMenu && !e.WithControl && e.Key == VIRTUAL_KEY.VK_TAB)
        {
            var focused = Window?.FocusedVisual;
            var childFocused = focused != null && IsChild(focused);
            if (childFocused)
            {
                var rowIndex = RowIndex;
#if NETFRAMEWORK
                PropertyValueVisual? sibling;
#else
                PropertyValueVisual<T>? sibling;
#endif
                if (e.WithShift)
                {
                    sibling = Parent.PropertyValueVisuals.Reverse().Where(pv => pv.RowIndex < rowIndex && pv.AllChildren.Any(c => c.IsFocusable)).FirstOrDefault();
                }
                else
                {
                    sibling = Parent.PropertyValueVisuals.Where(pv => pv.RowIndex > rowIndex && pv.AllChildren.Any(c => c.IsFocusable)).FirstOrDefault();
                }
                if (sibling != null)
                    return sibling.ScrollIntoView();
            }
        }
        return false;
    }

    /// <summary>
    /// Scrolls the current property visuals into view if the parent <see cref="PropertyGrid"/> is contained by a <see cref="ScrollViewer"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the element is successfully scrolled into view; otherwise, <see langword="false"/> if
    /// the element is not contained within a <see cref="ScrollViewer"/>.</returns>
    public virtual bool ScrollIntoView()
    {
        // we expect the visual tree to be: PropertyValueVisual -> Grid -> PropertyGrid -> ScrollViewer
        if (Parent?.Parent?.Parent is not ScrollViewer sv)
            return false;

        var firstFocusable = AllChildren.FirstOrDefault(c => c.IsFocusable);
        if (sv.ArrangedRect.IsValid)
        {
            sv.VerticalOffset = ArrangedRect.bottom + Margin.top - sv.ArrangedRect.Height;
            sv.HorizontalOffset = ArrangedRect.right + Margin.left - sv.ArrangedRect.Width;
            firstFocusable?.Focus();
        }
        else
        {
            sv.Arranged += onArranged;
        }
        return true;

        void onArranged(object? sender, EventArgs e)
        {
            sv.Arranged -= onArranged;
            ScrollIntoView();
            firstFocusable?.Focus();
        }
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
