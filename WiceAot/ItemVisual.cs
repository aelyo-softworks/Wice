namespace Wice;

/// <summary>
/// Visual representing a selectable item container.
/// </summary>
public partial class ItemVisual : Border, IOneChildParent, IFocusableParent, ISelectable
{
    /// <summary>
    /// Gets the dependency property that represents the selection state of an item.
    /// </summary>
    public static VisualProperty IsSelectedProperty { get; } = VisualProperty.Add(typeof(ItemVisual), nameof(IsSelected), VisualPropertyInvalidateModes.Measure, false);

    /// <summary>
    /// Occurs when the selection state changes.
    /// </summary>
    public event EventHandler<ValueEventArgs<bool>>? IsSelectedChanged;

    private INotifyPropertyChanged? _notifyPropertyChanged;

    /// <summary>
    /// Initializes a new <see cref="ItemVisual"/> with <see cref="RaiseIsSelectedChanged"/> set to true.
    /// </summary>
    public ItemVisual()
    {
        RaiseIsSelectedChanged = true;
    }

    /// <summary>
    /// Virtual hook to enable/disable event emission for selection changes.
    /// </summary>
    protected virtual bool RaiseIsSelectedChanged { get; set; }
    bool ISelectable.RaiseIsSelectedChanged { get => RaiseIsSelectedChanged; set => RaiseIsSelectedChanged = value; }

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

    Visual? IFocusableParent.FocusableVisual => Child;
    float? IFocusableParent.FocusOffset => null;

#if !NETFRAMEWORK
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    Type? IFocusableParent.FocusVisualShapeType => null;

    /// <inheritdoc/>
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
            _notifyPropertyChanged?.PropertyChanged -= OnDataPropertyChanged;
            _notifyPropertyChanged = null;

            _notifyPropertyChanged = value as INotifyPropertyChanged;
            _notifyPropertyChanged?.PropertyChanged += OnDataPropertyChanged;
        }

        return true;
    }

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

    /// <inheritdoc/>
    protected override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (Parent?.IsEnabled == false)
            return;

        IsSelected = !IsSelected;
        Child?.Focus();
        base.OnMouseButtonDown(sender, e);
    }
}
