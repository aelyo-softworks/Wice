namespace Wice;

/// <summary>
/// Represents a control that allows users to select a value from a specified range by sliding a handle along a track.
/// </summary>
/// <typeparam name="T">The type of the value that the slider can represent, constrained to types that implement the INumber interface.</typeparam>
public partial class Slider<
#if !NETFRAMEWORK
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
T> : Dock, IValueable where T : INumber<T>, IMinMaxValue<T>
{
    /// <summary>
    /// Dynamic property descriptor for <see cref="Value"/>.
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// </summary>
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add<T>(typeof(Slider<T>), nameof(Value), VisualPropertyInvalidateModes.Render, T.Zero, changed: OnValueChanged);

    /// <summary>
    /// Dynamic property descriptor for <see cref="MinValue"/>.
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// </summary>
    public static VisualProperty MinValueProperty { get; } = VisualProperty.Add<T>(typeof(Slider<T>), nameof(MinValue), VisualPropertyInvalidateModes.Render, T.Zero, changed: OnMinValueChanged);

    /// <summary>
    /// Dynamic property descriptor for <see cref="MaxValue"/>.
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// </summary>
    public static VisualProperty MaxValueProperty { get; } = VisualProperty.Add<T>(typeof(Slider<T>), nameof(MaxValue), VisualPropertyInvalidateModes.Render, GetDefaultMaxValue(), changed: OnMaxValueChanged);

    private static void OnValueChanged(BaseObject obj, object? newValue, object? oldValue) => ((Slider<T>)obj).OnValueChanged();
    private static void OnMinValueChanged(BaseObject obj, object? newValue, object? oldValue) => ((Slider<T>)obj).OnMinValueChanged();
    private static void OnMaxValueChanged(BaseObject obj, object? newValue, object? oldValue) => ((Slider<T>)obj).OnMaxValueChanged();

    private static T GetDefaultMaxValue()
    {
        if (typeof(T) == typeof(short) || typeof(T) == typeof(ushort) ||
            typeof(T) == typeof(int) || typeof(T) == typeof(uint) ||
            typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte) ||
            typeof(T) == typeof(long) || typeof(T) == typeof(ulong))
            return (T)(object)100;

        if (typeof(T) == typeof(float))
            return (T)(object)1f;

        if (typeof(T) == typeof(double))
            return (T)(object)1d;

        if (typeof(T) == typeof(decimal))
            return (T)(object)1m;

        return T.MaxValue;
    }

    /// <summary>
    /// Occurs when <see cref="Value"/> changes.
    /// </summary>
    public event EventHandler<ValueEventArgs<T>>? ValueChanged;

    private event EventHandler<ValueEventArgs>? _valueChanged;
    event EventHandler<ValueEventArgs> IValueable.ValueChanged { add { _valueChanged += value; } remove { _valueChanged -= value; } }

    /// <summary>
    /// Occurs when <see cref="ISelectable.IsSelected"/> changes and <see cref="ISelectable.RaiseIsSelectedChanged"/> is true.
    /// Mirrors <see cref="ValueChanged"/>.
    /// </summary>
    public event EventHandler<ValueEventArgs<bool>>? IsSelectedChanged;

    bool IValueable.CanChangeValue { get => IsEnabled; set => IsEnabled = value; }
    object? IValueable.Value => Value;
    bool IValueable.TrySetValue(object? value)
    {
        if (value is T t)
        {
            Value = t;
            return true;
        }

        if (Conversions.TryChangeType<T>(value, out var v))
        {
            Value = v!;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Initializes a new instance of the Slider class and sets up the visual elements representing the minimum and
    /// maximum values, as well as the track visuals.
    /// </summary>
    public Slider()
    {
        MinValueVisual = CreateMinValueVisual();
        if (MinValueVisual != null)
        {
#if DEBUG
            MinValueVisual.Name = nameof(MinValueVisual);
#endif
            SetDockType(MinValueVisual, DockType.Left);
            Children.Add(MinValueVisual);
        }

        MinTrackVisual = CreateMinTrackVisual();
        if (MinTrackVisual != null)
        {
#if DEBUG
            MinTrackVisual.Name = nameof(MinTrackVisual);
#endif
            SetDockType(MinTrackVisual, DockType.Left);
            Children.Add(MinTrackVisual);
        }

        Thumb = CreateThumb();
        if (Thumb != null)
        {
#if DEBUG
            Thumb.Name = nameof(Thumb);
#endif
            SetDockType(Thumb, DockType.Left);
            Children.Add(Thumb);
        }

        MaxValueVisual = CreateMaxValueVisual();
        if (MaxValueVisual != null)
        {
#if DEBUG
            MaxValueVisual.Name = nameof(MaxValueVisual);
#endif
            SetDockType(MaxValueVisual, DockType.Right);
            Children.Add(MaxValueVisual);
        }

        MaxTrackVisual = CreateMaxTrackVisual();
        if (MaxTrackVisual != null)
        {
#if DEBUG
            MaxTrackVisual.Name = nameof(MaxTrackVisual);
#endif
            SetDockType(MaxTrackVisual, DockType.Right);
            Children.Add(MaxTrackVisual);
        }

        TicksVisual = CreateTicksVisual();
        if (TicksVisual != null)
        {
#if DEBUG
            TicksVisual.Name = nameof(TicksVisual);
#endif
            Children.Add(TicksVisual);
        }
    }

    /// <summary>
    /// Gets or sets the value of the control.
    /// Changing this property updates invalidates rendering and raises <see cref="ValueChanged"/>.
    /// </summary>
    [Category(CategoryBehavior)]
    public T Value { get => (T)GetPropertyValue(ValueProperty)!; set => SetPropertyValue(ValueProperty, value); }

    /// <summary>
    /// Gets or sets the minimum value of the control.
    /// Changing this property updates invalidates rendering and raises <see cref="ValueChanged"/>.
    /// </summary>
    [Category(CategoryBehavior)]
    public T MinValue { get => (T)GetPropertyValue(MinValueProperty)!; set => SetPropertyValue(MinValueProperty, value); }

    /// <summary>
    /// Gets or sets the maximum value of the control.
    /// Changing this property updates invalidates rendering and raises <see cref="ValueChanged"/>.
    /// </summary>
    [Category(CategoryBehavior)]
    public T MaxValue { get => (T)GetPropertyValue(MaxValueProperty)!; set => SetPropertyValue(MaxValueProperty, value); }

    [Browsable(false)]
    public Visual? MinValueVisual { get; }

    [Browsable(false)]
    public Visual? MaxValueVisual { get; }

    [Browsable(false)]
    public Visual? MinTrackVisual { get; }

    [Browsable(false)]
    public Thumb? Thumb { get; }

    [Browsable(false)]
    public Visual? MaxTrackVisual { get; }

    [Browsable(false)]
    public Visual? TicksVisual { get; set; }

    protected virtual Visual CreateMinValueVisual()
    {
        var tb = new TextBox
        {
            Text = MinValue.ToString() ?? string.Empty
        };
        return tb;
    }

    protected virtual Visual CreateMinTrackVisual()
    {
        var rr = new RoundedRectangle();
        rr.VerticalAlignment = Alignment.Center;
        rr.Height = 4;
        return rr;
    }

    protected virtual Thumb CreateThumb()
    {
        var th = new Thumb();
        th.DragDelta += OnThumbDragDelta;
        th.VerticalAlignment = Alignment.Center;
        return th;
    }

    protected virtual void OnThumbDragDelta(object? sender, DragEventArgs e)
    {
        if (!Conversions.TryChangeType<float>(MaxValue - MinValue, out var range) ||
            range == 0 ||
            !Conversions.TryChangeType<float>(MinValue, out var minValue) ||
            !Conversions.TryChangeType<float>(Value, out var value))
            return;

        var minTrackWidth = MinTrackVisual.ArrangedRect.Width;
        var thumbWidth = Thumb.ArrangedRect.Width;
        var maxTrackWidth = MaxTrackVisual.ArrangedRect.Width;

        var totalTrackWidth = minTrackWidth + thumbWidth + maxTrackWidth;
        if (totalTrackWidth == 0)
            return;


        var minValueWidth = MinValueVisual.ArrangedRect.Width;

        var valueChange = minValueWidth * range / totalTrackWidth;
        if (!Conversions.TryChangeType<T>(valueChange, out var valueChangeT))
            return;

        Application.Trace($"Thumb dragged: value change {valueChangeT}, range {range}, totalTrackWidth {totalTrackWidth}, minValueWidth {minValueWidth}");
        Value = MinValue + valueChangeT;
    }

    protected virtual Visual CreateMaxTrackVisual()
    {
        var rr = new RoundedRectangle();
        rr.VerticalAlignment = Alignment.Center;
        rr.Height = 4;
        return rr;
    }

    protected virtual Visual CreateMaxValueVisual()
    {
        var tb = new TextBox
        {
            Text = MaxValue.ToString() ?? string.Empty,
        };
        return tb;
    }

    protected virtual Visual CreateTicksVisual()
    {
        return null;
    }

    protected override void Render()
    {
        base.Render();

        if (Compositor == null)
            return;

        var theme = GetWindowTheme();
        if (MinTrackVisual is Shape min)
        {
            min.FillBrush = Compositor.CreateColorBrush(theme.SelectedColor.ToColor());
        }

        if (MaxTrackVisual is Shape max)
        {
            max.FillBrush = Compositor.CreateColorBrush(theme.UnselectedColor.ToColor());
        }
    }

    protected virtual void OnValueChanged(object? sender, ValueEventArgs<T> e)
    {
        ValueChanged?.Invoke(sender, e);
        _valueChanged?.Invoke(sender, e);
    }

    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        if (Conversions.TryChangeType<float>(MaxValue - MinValue, out var range) &&
            range != 0 &&
            Conversions.TryChangeType<float>(MinValue, out var minValue) &&
            Conversions.TryChangeType<float>(Value, out var value))
        {
            // measure tracks based on value position
            MinValueVisual.Measure(constraint);
            MaxValueVisual.Measure(constraint);
            Thumb.Measure(constraint);
            var totalTrackWidth = constraint.width - MinValueVisual.DesiredSize.width - MaxValueVisual.DesiredSize.width - Thumb.DesiredSize.width;

            var minTrackWidth = totalTrackWidth * (value - minValue) / range;
            var maxTrackWidth = totalTrackWidth - minTrackWidth;
            if (MinTrackVisual != null)
            {
                MinTrackVisual.Width = minTrackWidth;
            }

            if (MaxTrackVisual != null)
            {
                MaxTrackVisual.Width = maxTrackWidth;
            }
        }

        var size = base.MeasureCore(constraint);
        return size;
    }

    private void OnValueChanged()
    {
        if (Value < MinValue)
        {
            Value = MinValue;
        }

        if (Value > MaxValue)
        {
            Value = MaxValue;
        }

        OnValueChanged(this, new ValueEventArgs<T>(Value));
    }

    protected virtual void OnMinValueChanged()
    {
        if (Value < MinValue)
        {
            Value = MinValue;
        }

        if (MinValue > MaxValue)
        {
            MaxValue = MinValue;
        }

        if (MinValueVisual is IValueable valueable)
        {
            valueable.TrySetValue(MinValue);
        }
    }

    protected virtual void OnMaxValueChanged()
    {
        if (Value > MaxValue)
        {
            MaxValue = Value;
        }

        if (MaxValue < MinValue)
        {
            MinValue = MaxValue;
        }

        if (MaxValueVisual is IValueable valueable)
        {
            valueable.TrySetValue(MaxValue);
        }
    }
}
