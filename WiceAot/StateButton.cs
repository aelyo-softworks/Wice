namespace Wice;

public partial class StateButton : ButtonBase, IValueable, ISelectable
{
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

    public event EventHandler<ValueEventArgs>? ValueChanged;
    public event EventHandler<ValueEventArgs<bool>>? IsSelectedChanged; // only for true/false values

    private readonly List<StateButtonState> _states = [];

    public StateButton()
    {
        Width = Application.CurrentTheme.BoxSize;
        Height = Application.CurrentTheme.BoxSize;
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

    [Category(CategoryBehavior)]
    public IReadOnlyList<StateButtonState> States => _states;

    [Category(CategoryBehavior)]
    public object? Value { get => GetPropertyValue(ValueProperty); set => SetPropertyValue(ValueProperty, value); }

    protected virtual void OnIsSelectedChanged(object? sender, ValueEventArgs<bool> e)
    {
        if (((ISelectable)this).RaiseIsSelectedChanged)
        {
            IsSelectedChanged?.Invoke(sender, e);
        }
    }

    public virtual void AddState(StateButtonState state)
    {
        ExceptionExtensions.ThrowIfNull(state, nameof(state));
        if (Parent != null)
            throw new WiceException("0017: Cannot add a state once attached to the UI tree.");

        _states.Add(state);
    }

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

    protected virtual void OnValueChanged(object sender, ValueEventArgs e) => ValueChanged?.Invoke(sender, e);

    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        UpdateValueState(e);
    }

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
}
