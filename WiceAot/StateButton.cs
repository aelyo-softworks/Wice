namespace Wice;

/// <summary>
/// A button that cycles through predefined logical states and exposes the current state via <see cref="Value"/>.
/// </summary>
public partial class StateButton : ButtonBase, IValueable, ISelectable
{
    /// <summary>
    /// Backing property for <see cref="Value"/>. Uses <see cref="ConvertValue(BaseObject, object?)"/> to validate/normalize input and
    /// invalidates measure on change to update layout accordingly.
    /// </summary>
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add<object>(typeof(StateButton), nameof(Value), VisualPropertyInvalidateModes.Measure, convert: ConvertValue);

    internal static object? ConvertValue(BaseObject obj, object? value)
    {
        var box = (StateButton)obj;
        foreach (var state in box.States)
        {
            if (state.Equals(value))
                return value;
        }
        throw new ArgumentOutOfRangeException(nameof(value));
    }

    /// <summary>
    /// Raised after <see cref="Value"/> changes (after layout invalidation is queued).
    /// </summary>
    public event EventHandler<ValueEventArgs>? ValueChanged;

    /// <summary>
    /// Raised when <see cref="ISelectable.IsSelected"/> changes. Only emitted for boolean values (true/false).
    /// </summary>
    public event EventHandler<ValueEventArgs<bool>>? IsSelectedChanged; // only for true/false values

    private readonly List<StateButtonState> _states = [];

    /// <summary>
    /// Initializes a new instance of <see cref="StateButton"/> and applies theme-dependent sizing when attached.
    /// </summary>
    public StateButton()
    {
        OnThemeDpiEvent(null, ThemeDpiEventArgs.FromWindow(Window));
    }

    bool ISelectable.RaiseIsSelectedChanged { get; set; }
    bool ISelectable.IsSelected
    {
        get => true.Equals(Value);
        set
        {
            foreach (var state in _states)
            {
                if (value.Equals(state.Value))
                {
                    Value = value;
                    break;
                }
            }
        }
    }

    bool IValueable.CanChangeValue { get => IsEnabled; set => IsEnabled = value; }
    bool IValueable.TrySetValue(object? value)
    {
        foreach (var state in States)
        {
            if (state.Equals(value))
            {
                Value = state;
                return true;
            }
        }
        return true;
    }

    /// <summary>
    /// Gets or sets whether this button sizes itself to the theme's box size when DPI/theme changes occur.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual bool AutoSize { get; set; } = true;

    /// <summary>
    /// Gets the collection of states this button can cycle through.
    /// </summary>
    [Category(CategoryBehavior)]
    public IReadOnlyList<StateButtonState> States => _states;

    /// <summary>
    /// Gets or sets the current state value. Must match one of the declared <see cref="States"/>.
    /// </summary>
    [Category(CategoryBehavior)]
    public object? Value { get => GetPropertyValue(ValueProperty); set => SetPropertyValue(ValueProperty, value); }

    /// <summary>
    /// Called when <see cref="ISelectable.IsSelected"/> changes and <see cref="ISelectable.RaiseIsSelectedChanged"/> is enabled.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    protected virtual void OnIsSelectedChanged(object? sender, ValueEventArgs<bool> e)
    {
        if (((ISelectable)this).RaiseIsSelectedChanged)
        {
            IsSelectedChanged?.Invoke(sender, e);
        }
    }

    /// <summary>
    /// Adds a new <paramref name="state"/> to the button. Must be called before the button is attached to the UI tree.
    /// </summary>
    /// <param name="state">The state to add.</param>
    public virtual void AddState(StateButtonState state)
    {
        ExceptionExtensions.ThrowIfNull(state, nameof(state));
        if (Parent != null)
            throw new WiceException("0017: Cannot add a state once attached to the UI tree.");

        _states.Add(state);
    }

    /// <inheritdoc/>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == ValueProperty)
        {
            UpdateValueState(EventArgs.Empty);
            OnValueChanged(this, new ValueEventArgs(value));
            if (true.Equals(value) || false.Equals(value))
            {
                OnIsSelectedChanged(this, new ValueEventArgs<bool>((bool)value));
            }
        }
        return true;
    }

    /// <summary>
    /// Ensures that the visual child corresponds to the current <see cref="Value"/> and normalizes <see cref="Value"/> to the matched state's <see cref="StateButtonState.Value"/>.
    /// </summary>
    /// <param name="e">The originating event that triggered the update.</param>
    protected virtual void UpdateValueState(EventArgs e)
    {
        var value = Value;
        StateButtonState? valueState = null;
        foreach (var state in States)
        {
            if (state.Equals(value))
            {
                valueState = state;
                break;
            }
        }

        if (valueState == null && States.Count > 0)
        {
            valueState = States[0];
        }

        if (valueState != null)
        {
            Child = valueState.CreateChild(this, e);
            Value = valueState.Value;
        }
    }

    /// <summary>
    /// Raises <see cref="ValueChanged"/>.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    protected virtual void OnValueChanged(object sender, ValueEventArgs e) => ValueChanged?.Invoke(sender, e);

    private StateButtonState? GetNextState()
    {
        if (States.Count <= 1)
            return null;

        var index = States.IndexOf(s => s.Equals(Value));
        if (index < 0)
            return States[0];

        if ((index + 1) < States.Count)
            return States[index + 1];

        return States[0];
    }

    /// <inheritdoc/>
    protected override void OnClick(object? sender, EventArgs e)
    {
        var state = GetNextState();
        if (state != null)
        {
            Child = state.CreateChild(this, e);
            Value = state.Value;
        }

        base.OnClick(sender, e);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
        UpdateValueState(e);
        Window!.ThemeDpiEvent += OnThemeDpiEvent;
    }

    /// <inheritdoc/>
    protected override void OnDetachingFromComposition(object? sender, EventArgs e)
    {
        base.OnDetachingFromComposition(sender, e);
        Window!.ThemeDpiEvent -= OnThemeDpiEvent;
    }

    /// <summary>
    /// Handles DPI/theme events to update size when <see cref="AutoSize"/> is enabled.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">DPI/theme event data.</param>
    protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        if (!AutoSize)
            return;

        var theme = GetWindowTheme();
        Width = theme.BoxSize;
        Height = theme.BoxSize;
    }
}
