namespace Wice;

/// <summary>
/// Visual representing a selectable item container.
/// - Exposes an IsSelected boolean state (dynamic VisualProperty) and raises <see cref="IsSelectedChanged"/> when it changes.
/// - When clicked (mouse button down) it toggles the selection and attempts to focus its single child.
/// - If bound to a data object implementing <see cref="INotifyPropertyChanged"/>, re-invokes <see cref="DataBinder"/>
///   to update the child when the data changes.
/// </summary>
public partial class ItemVisual : Border, IOneChildParent, IFocusableParent, ISelectable
{
    /// <summary>
    /// Dynamic visual property backing <see cref="IsSelected"/>. Triggers a Measure invalidation when changed.
    /// Default value is <see langword="false"/>.
    /// </summary>
    public static VisualProperty IsSelectedProperty { get; } = VisualProperty.Add(typeof(ItemVisual), nameof(IsSelected), VisualPropertyInvalidateModes.Measure, false);

    /// <summary>
    /// Raised after <see cref="IsSelected"/> changes and when <see cref="RaiseIsSelectedChanged"/> is <see langword="true"/>.
    /// </summary>
    public event EventHandler<ValueEventArgs<bool>>? IsSelectedChanged;

    /// <summary>
    /// Holds the current data item cast to <see cref="INotifyPropertyChanged"/> (when available) in order to
    /// subscribe to its <see cref="INotifyPropertyChanged.PropertyChanged"/> and re-run the <see cref="DataBinder"/>.
    /// Lifecycle:
    /// - Unsubscribed when the Data property changes away from an observable instance.
    /// - Subscribed when the Data property is set to an observable instance.
    /// </summary>
    private INotifyPropertyChanged? _notifyPropertyChanged;

    /// <summary>
    /// Initializes a new <see cref="ItemVisual"/> with <see cref="RaiseIsSelectedChanged"/> set to true.
    /// </summary>
    public ItemVisual()
    {
        RaiseIsSelectedChanged = true;
    }

    /// <summary>
    /// Controls whether <see cref="IsSelectedChanged"/> is raised when <see cref="IsSelected"/> changes.
    /// </summary>
    bool ISelectable.RaiseIsSelectedChanged { get => RaiseIsSelectedChanged; set => RaiseIsSelectedChanged = value; }

    /// <summary>
    /// Virtual hook to enable/disable event emission for selection changes.
    /// </summary>
    protected virtual bool RaiseIsSelectedChanged { get; set; }

    /// <summary>
    /// Provides data-binding logic used to map the current data item onto the child visual.
    /// When the data object implements <see cref="INotifyPropertyChanged"/>, the binder is invoked on each change.
    /// </summary>
    [Browsable(false)]
    public virtual DataBinder? DataBinder { get; set; }

    /// <summary>
    /// Gets or sets the selection state of the item. Backed by <see cref="IsSelectedProperty"/>.
    /// Changing this value triggers a layout measure invalidation and, by default, raises <see cref="IsSelectedChanged"/>.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool IsSelected { get => (bool)GetPropertyValue(IsSelectedProperty)!; set => SetPropertyValue(IsSelectedProperty, value); }

    /// <summary>
    /// Returns the focusable visual for this container, which is its single child.
    /// </summary>
    Visual? IFocusableParent.FocusableVisual => Child;

#if !NETFRAMEWORK
    /// <summary>
    /// Returns the shape type to use for focus adorner. Null means default focus visuals.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    Type? IFocusableParent.FocusVisualShapeType => null;

    /// <summary>
    /// Optional offset for focus adorner. Null means default offset.
    /// </summary>
    float? IFocusableParent.FocusOffset => null;

    /// <summary>
    /// Overrides property setting to:
    /// - Raise <see cref="IsSelectedChanged"/> when <see cref="IsSelectedProperty"/> changes (when enabled).
    /// - Manage subscriptions to the data object's <see cref="INotifyPropertyChanged"/> when <see cref="Visual.DataProperty"/> changes,
    ///   invoking <see cref="DataBinder"/> on each data change to update the child.
    /// All other properties are delegated to the base implementations.
    /// </summary>
    /// <param name="property">The property being set.</param>
    /// <param name="value">The new value.</param>
    /// <param name="options">Set options.</param>
    /// <returns>True if the stored value changed; otherwise false.</returns>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == IsSelectedProperty)
        {
            if (RaiseIsSelectedChanged)
            {
                OnIsSelectedChanged(this, new ValueEventArgs<bool>((bool)value!));
            }
        }
        else if (property == DataProperty)
        {
            if (_notifyPropertyChanged != null)
            {
                _notifyPropertyChanged.PropertyChanged -= OnDataPropertyChanged;
                _notifyPropertyChanged = null;
            }

            _notifyPropertyChanged = value as INotifyPropertyChanged;
            if (_notifyPropertyChanged != null)
            {
                _notifyPropertyChanged.PropertyChanged += OnDataPropertyChanged;
            }
        }

        return true;
    }

    /// <summary>
    /// Handles <see cref="INotifyPropertyChanged.PropertyChanged"/> for the current data object.
    /// Re-invokes the <see cref="DataBinder"/>'s DataItemVisualBinder with a context targeting the child visual.
    /// </summary>
    private void OnDataPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var vb = DataBinder?.DataItemVisualBinder;
        if (vb != null)
        {
            var ctx = new DataBindContext(_notifyPropertyChanged)
            {
                DataVisual = Child
            };
            vb(ctx);
        }
    }

    /// <summary>
    /// Virtual hook invoked when <see cref="IsSelected"/> changes. Raises <see cref="IsSelectedChanged"/>.
    /// </summary>
    protected virtual void OnIsSelectedChanged(object? sender, ValueEventArgs<bool> e) => IsSelectedChanged?.Invoke(sender, e);

    /// <summary>
    /// Toggles <see cref="IsSelected"/> when the item is clicked, focuses the child visual, and forwards the event to the base.
    /// When the parent is disabled, the interaction is ignored.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Mouse button event args.</param>
    protected override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (Parent?.IsEnabled == false)
            return;

        Application.Trace("ItemVisual");
        // e.Handled = true;
        IsSelected = !IsSelected;
        Child?.Focus();
        base.OnMouseButtonDown(sender, e);
    }
}
