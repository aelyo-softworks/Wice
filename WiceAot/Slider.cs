namespace Wice;

/// <summary>
/// Represents a visual that allows users to select a value from a specified range by sliding a handle along a track.
/// Default behavior requires the number type T to be convertible to System.Single for proper measurement and value calculations,
/// otherwise you should override at least the TryConvertToSingle and TryConvertFromSingle methods.
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
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add<T>(typeof(Slider<T>), nameof(Value), VisualPropertyInvalidateModes.Measure, T.Zero, changed: OnValueChanged);

    /// <summary>
    /// Dynamic property descriptor for <see cref="MinValue"/>.
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// </summary>
    public static VisualProperty MinValueProperty { get; } = VisualProperty.Add<T>(typeof(Slider<T>), nameof(MinValue), VisualPropertyInvalidateModes.Measure, T.Zero, changed: OnMinValueChanged);

    /// <summary>
    /// Dynamic property descriptor for <see cref="MaxValue"/>.
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// Default value is determined by the type T, with numeric types defaulting to 100 or 1, and other types defaulting to T.MaxValue.
    /// </summary>
    public static VisualProperty MaxValueProperty { get; } = VisualProperty.Add<T>(typeof(Slider<T>), nameof(MaxValue), VisualPropertyInvalidateModes.Measure, GetDefaultMaxValue(), changed: OnMaxValueChanged);

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
    /// Gets or sets whether the visual sizes itself based on theme metrics on DPI/theme changes.
    /// Default is true.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual bool AutoSize { get; set; } = true;

    /// <summary>
    /// Gets or sets the value of the visual.
    /// Changing this property updates invalidates rendering and raises <see cref="ValueChanged"/>.
    /// </summary>
    [Category(CategoryBehavior)]
    public T Value { get => (T)GetPropertyValue(ValueProperty)!; set => SetPropertyValue(ValueProperty, value); }

    /// <summary>
    /// Gets or sets the minimum value of the visual.
    /// </summary>
    [Category(CategoryBehavior)]
    public T MinValue { get => (T)GetPropertyValue(MinValueProperty)!; set => SetPropertyValue(MinValueProperty, value); }

    /// <summary>
    /// Gets or sets the maximum value of the visual.
    /// Default value is determined by the type T, with numeric types defaulting to 100 (integer numbers) or 1 (floating numbers), and other types defaulting to T.MaxValue.
    /// </summary>
    [Category(CategoryBehavior)]
    public T MaxValue { get => (T)GetPropertyValue(MaxValueProperty)!; set => SetPropertyValue(MaxValueProperty, value); }

    /// <summary>
    /// Gets the visual representation associated with the minimum value of the visual.
    /// </summary>
    [Browsable(false)]
    public Visual? MinValueVisual { get; }

    /// <summary>
    /// Gets the visual representation associated with the maximum value of the visual.
    /// </summary>
    [Browsable(false)]
    public Visual? MaxValueVisual { get; }

    /// <summary>
    /// Gets the visual representation associated with the minimum track of the visual.
    /// </summary>
    [Browsable(false)]
    public Visual? MinTrackVisual { get; }

    /// <summary>
    /// Gets the current Thumb instance associated with the visual.
    /// </summary>
    [Browsable(false)]
    public Thumb? Thumb { get; }

    /// <summary>
    /// Gets the visual representation associated with the maximum track of the visual.
    /// </summary>
    [Browsable(false)]
    public Visual? MaxTrackVisual { get; }

    /// <summary>
    /// Gets the visual representation associated with the ticks of the visual.
    /// </summary>
    [Browsable(false)]
    public Visual? TicksVisual { get; set; }

    /// <summary>
    /// Creates a visual representation of the minimum value. By default, it's using a TextBox visual.
    /// </summary>
    /// <remarks>This method is intended to be overridden in derived classes to provide custom visualizations
    /// of the minimum value. The returned visual can be styled or modified further as needed.</remarks>
    /// <returns>A visual displaying the minimum value as text.</returns>
    protected virtual Visual CreateMinValueVisual()
    {
        var tb = new TextBox
        {
            Text = MinValue.ToString() ?? string.Empty,
            Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER,
            ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER
        };
        return tb;
    }

    /// <summary>
    /// Creates a visual representation of the minimum track. By default, it's using a RoundedRectangle visual.
    /// </summary>
    /// <remarks>This method is intended to be overridden in derived classes to provide custom visualizations
    /// of the minimum track. The returned visual can be styled or modified further as needed.</remarks>
    /// <returns>A visual displaying the minimum track as text.</returns>
    protected virtual Visual CreateMinTrackVisual()
    {
        var rr = new RoundedRectangle
        {
            VerticalAlignment = Alignment.Center,
        };
        return rr;
    }

    /// <summary>
    /// Creates a new instance of the Thumb visual that is configured to handle drag operations and is vertically
    /// centered.
    /// </summary>
    /// <remarks>Derived classes can override this method to customize the creation and configuration of the
    /// Thumb control. The returned Thumb has its DragDelta event attached to the OnThumbDragDelta handler, enabling
    /// drag functionality.</remarks>
    /// <returns>A Thumb visual that responds to drag events and is aligned to the vertical center.</returns>
    protected virtual Thumb CreateThumb()
    {
        var th = new Thumb
        {
            VerticalAlignment = Alignment.Center
        };
        th.DragDelta += OnThumbDragDelta;
        return th;
    }

    /// <summary>
    /// Handles the drag delta event for the thumb control, updating the slider's value based on the user's drag
    /// position.
    /// </summary>
    /// <param name="sender">The source of the event, typically the thumb visual being dragged.</param>
    /// <param name="e">The event data containing information about the drag operation, including the current mouse position.</param>
    protected virtual void OnThumbDragDelta(object? sender, DragEventArgs e)
    {
        if (!TryConvertToSingle(MaxValue - MinValue, out var range))
            return;

        var minValueWidth = MinValueVisual?.ArrangedRect.Width ?? 0;
        var minTrackWidth = MinTrackVisual?.ArrangedRect.Width ?? 0;
        var thumbWidth = Thumb?.ArrangedRect.Width ?? 0;
        var maxTrackWidth = MaxTrackVisual?.ArrangedRect.Width ?? 0;

        var totalTrackWidth = minTrackWidth + thumbWidth + maxTrackWidth;
        if (totalTrackWidth == 0)
            return;

        var valuePosition = e.X - minValueWidth - thumbWidth / 2;
        var valueChange = valuePosition * range / totalTrackWidth;
        if (!TryConvertFromSingle(valueChange, out var valueChangeT))
            return;

        Value = MinValue + valueChangeT!;
    }

    /// <summary>
    /// Attempts to convert the specified value to a single-precision floating-point number.
    /// </summary>
    /// <param name="value">The value to convert to a single-precision floating-point number.</param>
    /// <param name="result">When this method returns, contains the converted single-precision floating-point value if the conversion
    /// succeeded; otherwise, zero.</param>
    /// <returns>true if the value was successfully converted to a single-precision floating-point number; otherwise, false.</returns>
    protected virtual bool TryConvertToSingle(T value, out float result) => Conversions.TryChangeType<float>(value, out result);

    /// <summary>
    /// Attempts to convert the specified single-precision floating-point value to the target type.
    /// </summary>
    /// <param name="value">The single-precision floating-point number to convert.</param>
    /// <param name="result">When this method returns, contains the converted value if the conversion succeeded; otherwise, null.</param>
    /// <returns>true if the conversion was successful; otherwise, false.</returns>
    protected virtual bool TryConvertFromSingle(float value, [NotNullWhen(true)] out T? result) => Conversions.TryChangeType(value, out result);

    /// <summary>
    /// Creates a visual representation of the maximum track. By default, it's using a RoundedRectangle visual.
    /// </summary>
    /// <remarks>This method is intended to be overridden in derived classes to provide custom visualizations
    /// of the maximum track. The returned visual can be styled or modified further as needed.</remarks>
    /// <returns>A visual displaying the maximum track as text.</returns>
    protected virtual Visual CreateMaxTrackVisual()
    {
        var rr = new RoundedRectangle
        {
            VerticalAlignment = Alignment.Center,
        };
        return rr;
    }

    /// <summary>
    /// Creates a visual representation of the maximum value. By default, it's using a TextBox visual.
    /// </summary>
    /// <remarks>This method is intended to be overridden in derived classes to provide custom visualizations
    /// of the maximum value. The returned visual can be styled or modified further as needed.</remarks>
    /// <returns>A visual displaying the maximum value as text.</returns>
    protected virtual Visual CreateMaxValueVisual()
    {
        var tb = new TextBox
        {
            Text = MaxValue.ToString() ?? string.Empty,
            Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER,
            ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER
        };
        return tb;
    }

    /// <summary>
    /// Creates a visual representation of the ticks for the slider visual.
    /// </summary>
    /// <returns>A visual representing the ticks.</returns>
    protected virtual Visual CreateTicksVisual()
    {
        return null!;
    }

    /// <inheritdoc />
    protected override void Render()
    {
        base.Render();

        if (Compositor == null)
            return;

        var theme = GetWindowTheme();
        if (MinTrackVisual is Shape min)
        {
            min.FillBrush = Compositor.CreateColorBrush(theme.SliderNearColor.ToColor());
        }

        if (MaxTrackVisual is Shape max)
        {
            max.FillBrush = Compositor.CreateColorBrush(theme.SliderFarColor.ToColor());
        }

        Thumb?.FillBrush = Compositor.CreateColorBrush(theme.SliderThumbColor.ToColor());
    }

    /// <summary>
    /// Raises the ValueChanged event to notify subscribers when the value has changed.
    /// </summary>
    /// <remarks>Derived classes can override this method to provide custom handling when the value changes.
    /// This method invokes both the ValueChanged event and any additional event handlers that may be
    /// registered.</remarks>
    /// <param name="sender">The source of the event, typically the object whose value has changed.</param>
    /// <param name="e">An instance of ValueEventArgs that contains the new value and any associated event data.</param>
    protected virtual void OnValueChanged(object? sender, ValueEventArgs<T> e)
    {
        ValueChanged?.Invoke(sender, e);
        _valueChanged?.Invoke(sender, e);
    }

    /// <inheritdoc />
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        if (TryConvertToSingle(MaxValue - MinValue, out var range) &&
            range != 0 &&
            TryConvertToSingle(MinValue, out var minValue) &&
            TryConvertToSingle(Value, out var value))
        {
            // measure tracks based on value position
            MinValueVisual?.Measure(constraint);
            MaxValueVisual?.Measure(constraint);
            Thumb?.Measure(constraint);

            var totalTrackWidth = constraint.width - (MinValueVisual?.DesiredSize.width ?? 0) - (MaxValueVisual?.DesiredSize.width ?? 0) - (Thumb?.DesiredSize.width ?? 0);
            var minTrackWidth = totalTrackWidth * (value - minValue) / range;
            var maxTrackWidth = totalTrackWidth - minTrackWidth;

            MinTrackVisual?.Width = minTrackWidth;
            MaxTrackVisual?.Width = maxTrackWidth;
        }

        return base.MeasureCore(constraint);
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

    /// <summary>
    /// Handles changes to the minimum value, ensuring that related properties remain within valid ranges and updating
    /// associated visuals as needed.
    /// </summary>
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

    /// <summary>
    /// Handles changes to the maximum value, ensuring that related properties remain within valid ranges and updating
    /// associated visuals as needed.
    /// </summary>
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

    /// <inheritdoc/>
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
        Window!.ThemeDpiEvent += OnThemeDpiEvent;
    }

    /// <inheritdoc/>
    protected override void OnDetachingFromComposition(object? sender, EventArgs e)
    {
        base.OnDetachingFromComposition(sender, e);
        Window!.ThemeDpiEvent -= OnThemeDpiEvent;
    }

    /// <summary>
    /// Applies automatic sizing based on current theme metrics when <see cref="AutoSize"/> is true.
    /// Sets width to twice the theme box size and height/knob size to the theme box size.
    /// </summary>
    /// <param name="sender">Event source (window).</param>
    /// <param name="e">Theme/DPI event args.</param>
    protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        if (!AutoSize)
            return;

        var theme = GetWindowTheme();
        var boxSize = theme.BoxSize;
        Height = boxSize;
        Thumb?.Height = boxSize;
        Thumb?.Width = boxSize;

        if (MinValueVisual is TextBox minTb)
        {
            minTb.Margin = D2D_RECT_F.Thickness(0, 0, theme.SliderPadding, 0);
        }

        if (MaxValueVisual is TextBox maxTb)
        {
            maxTb.Margin = D2D_RECT_F.Thickness(theme.SliderPadding, 0, 0, 0);
        }

        if (MinTrackVisual is RoundedRectangle minR)
        {
            minR.CornerRadius = new Vector2(theme.RoundedButtonCornerRadius);
            minR.Height = boxSize / 2;
            minR.RenderOffset = new Vector3(theme.RoundedButtonCornerRadius, 0, 0);
            minR.ZIndex = -1;
        }

        if (MaxTrackVisual is RoundedRectangle maxR)
        {
            maxR.CornerRadius = new Vector2(theme.RoundedButtonCornerRadius);
            maxR.Height = boxSize / 2;

            // we don't want to see the rounded corner on the left side of the max track,
            // so we offset it to the left by the corner radius and set ZIndex to -1 to render it behind the thumb
            maxR.RenderOffset = new Vector3(-theme.RoundedButtonCornerRadius, 0, 0);
            maxR.ZIndex = -1;
        }
    }
}
