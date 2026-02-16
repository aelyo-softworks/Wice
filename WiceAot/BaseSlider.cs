namespace Wice;

/// <summary>
/// Represents a base visual that allows users to select a value from a specified range by sliding a handle along a track.
/// Default behavior requires the number type T to be convertible to System.Single for proper measurement and value calculations,
/// otherwise you should override at least the TryConvertToSingle and TryConvertFromSingle methods.
/// </summary>
/// <typeparam name="T">The type of the value that the slider can represent, constrained to types that implement the INumber interface.</typeparam>
public partial class BaseSlider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T> : Dock, IValueable where T : INumber<T>, IMinMaxValue<T>
{
    /// <summary>
    /// Dynamic property descriptor for <see cref="Value"/>.
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// </summary>
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add(typeof(BaseSlider<T>), nameof(Value), VisualPropertyInvalidateModes.Measure, T.Zero, changed: OnValueChanged);

    /// <summary>
    /// Dynamic property descriptor for <see cref="Step"/>.
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// Default value is determined by the type T, with floating-point types defaulting to 0.1 and other numeric types defaulting to 1.
    /// </summary>
    public static VisualProperty StepProperty { get; } = VisualProperty.Add(typeof(BaseSlider<T>), nameof(Step), VisualPropertyInvalidateModes.Render, GetDefaultStepValue(), changed: OnMinValueChanged);

    /// <summary>
    /// Dynamic property descriptor for <see cref="MinValue"/>.
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// </summary>
    public static VisualProperty MinValueProperty { get; } = VisualProperty.Add(typeof(BaseSlider<T>), nameof(MinValue), VisualPropertyInvalidateModes.Measure, T.Zero, changed: OnMinValueChanged);

    /// <summary>
    /// Dynamic property descriptor for <see cref="MaxValue"/>.
    /// Changing this property invalidates rendering (<see cref="VisualPropertyInvalidateModes.Render"/>).
    /// Default value is determined by the type T, with numeric types defaulting to 100 (integer types) or 1 (floating types), and other types defaulting to T.MaxValue.
    /// </summary>
    public static VisualProperty MaxValueProperty { get; } = VisualProperty.Add(typeof(BaseSlider<T>), nameof(MaxValue), VisualPropertyInvalidateModes.Measure, GetDefaultMaxValue(), changed: OnMaxValueChanged);

    private static void OnValueChanged(BaseObject obj, object? newValue, object? oldValue) => ((BaseSlider<T>)obj).OnValueChanged();
    private static void OnMinValueChanged(BaseObject obj, object? newValue, object? oldValue) => ((BaseSlider<T>)obj).OnMinValueChanged();
    private static void OnMaxValueChanged(BaseObject obj, object? newValue, object? oldValue) => ((BaseSlider<T>)obj).OnMaxValueChanged();

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

    private static T GetDefaultStepValue()
    {
        if (typeof(T) == typeof(float))
            return (T)(object).01f;

        if (typeof(T) == typeof(double))
            return (T)(object).01d;

        if (typeof(T) == typeof(decimal))
            return (T)(object).01m;

        return T.One;
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

    private SliderValueWindow? _valueWindow;
    private Timer? _hideValueWindow;

    /// <summary>
    /// Initializes a new instance of the Slider class and sets up the visual elements representing the minimum and
    /// maximum values, as well as the track visuals.
    /// </summary>
    /// <param name="orientation">The orientation of the slider, which determines the layout of the visuals and the direction of value changes.</param>
    public BaseSlider(Orientation orientation)
    {
        Orientation = orientation;
        IsFocusable = true;
        MinValueVisual = CreateMinValueVisual();
        if (MinValueVisual != null)
        {
#if DEBUG
            MinValueVisual.Name = nameof(MinValueVisual);
#endif
            Children.Add(MinValueVisual);
        }

        MinTrackVisual = CreateMinTrackVisual();
        if (MinTrackVisual != null)
        {
#if DEBUG
            MinTrackVisual.Name = nameof(MinTrackVisual);
#endif
            Children.Add(MinTrackVisual);
        }

        Thumb = CreateThumb();
        if (Thumb != null)
        {
#if DEBUG
            Thumb.Name = nameof(Thumb);
#endif
            Children.Add(Thumb);

            if (Thumb is IThumb th)
            {
                th.DragDelta += OnThumbDragDelta;
                th.DragStarted += OnThumbDragStarted;
                th.DragCompleted += OnThumbDragCompleted;
            }
        }

        MaxValueVisual = CreateMaxValueVisual();
        if (MaxValueVisual != null)
        {
#if DEBUG
            MaxValueVisual.Name = nameof(MaxValueVisual);
#endif
            Children.Add(MaxValueVisual);
        }

        MaxTrackVisual = CreateMaxTrackVisual();
        if (MaxTrackVisual != null)
        {
#if DEBUG
            MaxTrackVisual.Name = nameof(MaxTrackVisual);
#endif
            Children.Add(MaxTrackVisual);
        }

        TicksVisual = CreateTicksVisual();
        if (TicksVisual != null)
        {
#if DEBUG
            TicksVisual.Name = nameof(TicksVisual);
#endif
            if (orientation == Orientation.Horizontal)
            {
                SetDockType(TicksVisual, DockType.Bottom);
            }
            else
            {
                SetDockType(TicksVisual, DockType.Right);
            }
            Children.Add(TicksVisual);
        }
    }

    /// <summary>
    /// Gets the visual direction.
    /// </summary>
    [Category(CategoryLayout)]
    public Orientation Orientation { get; }

    /// <summary>
    /// Gets or sets whether the visual sizes itself based on theme metrics on DPI/theme changes.
    /// Default is true.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual bool AutoSize { get; set; } = true;


    // max│                   x      
    //    │                   x      
    //    │                   x
    //  v │                   x
    //  a │                   x
    //  l │                  x
    //  u │                xx
    //  e │              xx          
    //    │           xxx            
    //    │        xxxx
    // min│ xxxxxxx                 
    //    └────────────────────      
    //     0     mouse pos    1
    /// <summary>
    /// Gets or sets a function that transforms the tracking position (normalized beween 0 and 1) to Value, normalized 0 (MinValue) and 1 (MaxValue).
    /// By default, the function is linear.
    /// If this is set, InverseValueFunc should also be set.
    /// </summary>
    [Browsable(false)]
    public virtual Func<float, float>? ValueFunc { get; set; }

    /// <summary>
    /// Gets or sets the function used to compute the inverse value for a given float input.
    /// By default, the function is linear.
    /// If this is set, ValueFunc should also be set.
    /// </summary>
    [Browsable(false)]
    public virtual Func<float, float>? InverseValueFunc { get; set; }

    /// <summary>
    /// Gets or sets the duration, in milliseconds, before the value window is automatically hidden.
    /// </summary>
    [Category(CategoryBehavior)]
    public int ValueWindowHideTimeout { get; set; } = 2000;

    /// <summary>
    /// Gets or sets the format string that determines how the value is converted to its string representation.
    /// </summary>
    [Category(CategoryBehavior)]
    public string? ValueStringFormat { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a window displaying the current value is shown when the control is
    /// hovered over.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual bool EnableValueWindow { get; set; } = true;

    /// <summary>
    /// Gets or sets the factor that determines the size of large incremental changes when interacting with the visual.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual float LargeStepFactor { get; set; } = 10;

    /// <summary>
    /// Gets or sets the value of the visual.
    /// Changing this property updates invalidates rendering and raises <see cref="ValueChanged"/>.
    /// </summary>
    [Category(CategoryBehavior)]
    public T Value { get => (T)GetPropertyValue(ValueProperty)!; set => SetPropertyValue(ValueProperty, value); }

    /// <summary>
    /// Gets or sets the step value of the visual.
    /// </summary>
    [Category(CategoryBehavior)]
    public T Step { get => (T)GetPropertyValue(StepProperty)!; set => SetPropertyValue(StepProperty, value); }

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
    /// Gets the current visual instance associated with this visual's thumb.
    /// </summary>
    [Browsable(false)]
    public Visual? Thumb { get; }

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
    /// Formats the specified value as a string using the defined format or a default format for numeric types.
    /// </summary>
    /// <returns>A string representation of the specified value, formatted according to the defined or default format.</returns>
    protected virtual string GetValueString(SliderValueContext context, T value)
    {
        var format = ValueStringFormat;
        if (format == null)
        {
            if (typeof(T) == typeof(float) ||
                typeof(T) == typeof(double) ||
                typeof(T) == typeof(decimal))
            {
                format = "{0:0.##}";
            }
            else
            {
                format = "{0}";
            }
        }
        return string.Format(format, value);
    }

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
            Text = GetValueString(SliderValueContext.MinValue, MinValue),
            HorizontalAlignment = Alignment.Near,
            VerticalAlignment = Alignment.Near,
            Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER,
            ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER
        };

        if (Orientation == Orientation.Horizontal)
        {
            SetDockType(tb, DockType.Left);
        }
        else
        {
            SetDockType(tb, DockType.Top);
        }
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
            HorizontalAlignment = Alignment.Near,
            VerticalAlignment = Alignment.Center,
        };

        if (Orientation == Orientation.Horizontal)
        {
            SetDockType(rr, DockType.Left);
        }
        else
        {
            SetDockType(rr, DockType.Top);
        }
        return rr;
    }

    /// <summary>
    /// Creates a new instance of the Thumb visual that is configured to handle drag operations and is vertically
    /// centered.
    /// </summary>
    /// <remarks>Derived classes can override this method to customize the creation and configuration of the
    /// Thumb visual. The returned Thumb has its DragDelta event attached to the OnThumbDragDelta handler, enabling
    /// drag functionality.</remarks>
    /// <returns>A Thumb visual that responds to drag events and is aligned to the vertical center.</returns>
    protected virtual Visual CreateThumb()
    {
        var th = new Thumb
        {
            HorizontalAlignment = Alignment.Near,
            VerticalAlignment = Alignment.Near,
            IsFocusable = false,
            ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, Value?.ToString() ?? string.Empty)
        };

        if (Orientation == Orientation.Horizontal)
        {
            SetDockType(th, DockType.Left);
        }
        else
        {
            SetDockType(th, DockType.Top);
        }
        return th;
    }

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
            HorizontalAlignment = Alignment.Far,
            VerticalAlignment = Alignment.Center,
        };

        if (Orientation == Orientation.Horizontal)
        {
            SetDockType(rr, DockType.Right);
        }
        else
        {
            SetDockType(rr, DockType.Bottom);
        }
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
            Text = GetValueString(SliderValueContext.MaxValue, MaxValue),
            HorizontalAlignment = Alignment.Far,
            VerticalAlignment = Alignment.Near,
            Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER,
            ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER,

            // vertical text would be configured like this
            //ReadingDirection = DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_TOP_TO_BOTTOM,
            //FlowDirection = DWRITE_FLOW_DIRECTION.DWRITE_FLOW_DIRECTION_RIGHT_TO_LEFT
        };

        if (Orientation == Orientation.Horizontal)
        {
            SetDockType(tb, DockType.Right);
        }
        else
        {
            SetDockType(tb, DockType.Bottom);
        }
        return tb;
    }

    /// <summary>
    /// Creates a visual representation of the ticks for the slider visual.
    /// </summary>
    /// <returns>A visual representing the ticks.</returns>
    protected virtual Visual CreateTicksVisual()
    {
        return null!;
        //var cv = new Canvas
        //{
        //    HorizontalAlignment = Alignment.Center,
        //    VerticalAlignment = Alignment.Center,
        //};
        //return cv;
    }

    /// <summary>
    /// Creates a new instance of the SliderValueWindow used to display the current value of the slider.
    /// </summary>
    /// <returns>A new instance of SliderValueWindow that represents the window for displaying slider values.</returns>
    protected virtual SliderValueWindow CreateValueWindow() => new(this);

    /// <summary>
    /// Displays a value window at the appropriate location relative to the control, using the current orientation and theme
    /// settings.
    /// </summary>
    protected virtual void ShowValueWindow()
    {
        if (_valueWindow != null)
            return;

        _valueWindow = CreateValueWindow();
        if (_valueWindow == null)
            return;

        _valueWindow.Show();
    }

    /// <summary>
    /// Updates the value window text to display the current value of the slider.
    /// </summary>
    protected virtual void UpdateValueWindow() => _valueWindow?.Update();

    /// <summary>
    /// Closes the currently displayed value window, if any, and releases any associated resources.
    /// </summary>
    protected virtual void CloseValueWindow()
    {
        _valueWindow?.Close();
        _valueWindow = null;
    }

    /// <inheritdoc/>
    protected internal override void IsFocusedChanged(bool newValue)
    {
        base.IsFocusedChanged(newValue);
        if (!newValue)
        {
            _hideValueWindow?.Dispose();
            _hideValueWindow = null;
        }
    }

    /// <summary>
    /// Invoked when a drag operation on the thumb control is completed.
    /// </summary>
    /// <param name="sender">The source of the event, typically the thumb control that was dragged.</param>
    /// <param name="e">An object that contains the event data.</param>
    protected virtual void OnThumbDragCompleted(object? sender, EventArgs e) => CloseValueWindow();

    /// <summary>
    /// Invoked when a drag operation on the thumb visual begins, allowing derived classes to handle the start of the
    /// drag event.
    /// </summary>
    /// <param name="sender">The source of the event, typically the thumb visual that initiated the drag operation.</param>
    /// <param name="e">A DragEventArgs object that contains the event data for the drag operation.</param>
    protected virtual void OnThumbDragStarted(object? sender, DragEventArgs e)
    {
        _hideValueWindow?.Dispose();
        Focus();
        if (!TryConvertToSingle(Value - MinValue, out var value))
            return;

        e.State.Tag = value;
        if (EnableValueWindow)
        {
            ShowValueWindow();
        }
    }

    /// <summary>
    /// Handles the drag delta event for the thumb visual, updating the slider's value based on the user's drag
    /// position.
    /// </summary>
    /// <param name="sender">The source of the event, typically the thumb visual being dragged.</param>
    /// <param name="e">The event data containing information about the drag operation, including the current mouse position.</param>
    protected virtual void OnThumbDragDelta(object? sender, DragEventArgs e)
    {
        if (e.State.Tag is not float startValue)
            return;

        if (!TryConvertToSingle(MaxValue - MinValue, out var range) || range == 0)
            return;

        float trackSize;
        if (Orientation == Orientation.Horizontal)
        {
            var minValueSize = (MinValueVisual?.ArrangedRect.Width).ToZeroIfNotSet();
            var maxValueSize = (MaxValueVisual?.ArrangedRect.Width).ToZeroIfNotSet();
            trackSize = ArrangedRect.Width - minValueSize - maxValueSize;
        }
        else
        {
            var minValueSize = (MinValueVisual?.ArrangedRect.Height).ToZeroIfNotSet();
            var maxValueSize = (MaxValueVisual?.ArrangedRect.Height).ToZeroIfNotSet();
            trackSize = ArrangedRect.Height - minValueSize - maxValueSize;
        }

        var deltaPos = Orientation == Orientation.Horizontal ? e.State.DeltaX : e.State.DeltaY;

        var valueFunc = ValueFunc ?? linear;
        var inverseFunc = InverseValueFunc ?? inverseLinear;

        // Convert starting normalized value to starting position using inverse function
        var startNormalizedValue = Math.Min(1, Math.Max(0, startValue / range));
        var startPosition = inverseFunc(startNormalizedValue);

        // Add delta to position
        var normPos = startPosition + (deltaPos / trackSize);
        normPos = Math.Min(1, Math.Max(0, normPos));

        // Apply value function to convert position to normalized value
        var normalizedValue = valueFunc(normPos);
        normalizedValue = Math.Min(1, Math.Max(0, normalizedValue));
        var newValue = normalizedValue * range;

        if (!TryConvertFromSingle(newValue, out var newValueT))
            return;

        Value = MinValue + newValueT;
    }

    //private static float linear(float p) => p * p;
    //private static float inverseLinear(float p) => MathF.Sqrt(p);
    private static float linear(float p) => p;
    private static float inverseLinear(float p) => p;

    /// <summary>
    /// Attempts to convert the specified value to a single-precision floating-point number.
    /// </summary>
    /// <param name="value">The value to convert to a single-precision floating-point number.</param>
    /// <param name="result">When this method returns, contains the converted single-precision floating-point value if the conversion
    /// succeeded; otherwise, zero.</param>
    /// <returns>true if the value was successfully converted to a single-precision floating-point number; otherwise, false.</returns>
    public virtual bool TryConvertToSingle(T value, out float result) => Conversions.TryChangeType<float>(value, out result);

    /// <summary>
    /// Attempts to convert the specified single-precision floating-point value to the target type.
    /// </summary>
    /// <param name="value">The single-precision floating-point number to convert.</param>
    /// <param name="result">When this method returns, contains the converted value if the conversion succeeded; otherwise, null.</param>
    /// <returns>true if the conversion was successful; otherwise, false.</returns>
    public virtual bool TryConvertFromSingle(float value, [NotNullWhen(true)] out T? result) => Conversions.TryChangeType(value, out result);

    /// <inheritdoc/>
    protected override void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsEnabled || !IsFocused)
            return;

        var step = Step;
        var ctrl = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_CONTROL);
        if (ctrl && TryConvertFromSingle(LargeStepFactor, out var large))
        {
            step *= large;
        }

        switch (e.Key)
        {
            case VIRTUAL_KEY.VK_LEFT:
            case VIRTUAL_KEY.VK_UP:
                Value -= step;
                e.Handled = true;
                break;

            case VIRTUAL_KEY.VK_RIGHT:
            case VIRTUAL_KEY.VK_DOWN:
                Value += step;
                e.Handled = true;
                break;

            case VIRTUAL_KEY.VK_HOME:
                Value = MinValue;
                e.Handled = true;
                break;

            case VIRTUAL_KEY.VK_END:
                Value = MaxValue;
                e.Handled = true;
                break;
        }

        if (e.Handled && EnableValueWindow)
        {
            ShowValueWindow();
            _hideValueWindow?.Dispose();
            if (ValueWindowHideTimeout > 0)
            {
                _hideValueWindow = new Timer(_ =>
                {
                    CloseValueWindow();
                    _hideValueWindow?.Dispose();
                    _hideValueWindow = null;
                }, null, ValueWindowHideTimeout, Timeout.Infinite);
            }
        }
        base.OnKeyDown(sender, e);
    }

    /// <inheritdoc />
    protected override void Render()
    {
        //if (TicksVisual != null)
        //{
        //    TicksVisual.RenderOffset = new Vector3(-MinTrackVisual.CompositionVisual.Size.X - Thumb.CompositionVisual.Size.X - MaxValueVisual.CompositionVisual.Size.X, 20, 0);
        //    TicksVisual.CompositionVisual.Size = CompositionVisual.Size;
        //}

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

        if (Thumb is Shape thumb)
        {
            thumb.FillBrush = Compositor.CreateColorBrush(theme.SliderThumbColor.ToColor());
        }
    }

    /// <summary>
    /// Raises the ValueChanged event to notify subscribers when the value has changed.
    /// </summary>
    /// <param name="sender">The source of the event, typically the object whose value has changed.</param>
    /// <param name="e">An instance of ValueEventArgs that contains the new value and any associated event data.</param>
    protected virtual void OnValueChanged(object? sender, ValueEventArgs<T> e)
    {
        ValueChanged?.Invoke(sender, e);
        _valueChanged?.Invoke(sender, e);
        UpdateValueWindow();
    }

    private static bool IsVerticalText(Visual visual)
    {
        if (visual is not TextBox tb)
            return false;

        return tb.ReadingDirection is DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_TOP_TO_BOTTOM or DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_BOTTOM_TO_TOP;
    }

    /// <inheritdoc />
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        // handle texts verticality to properly measure width and height, since vertical text would swap those
        var width = Width;
        var height = Height;
        if (MinValueVisual != null)
        {
            MinValueVisual.Measure(constraint);
            if (!IsVerticalText(MinValueVisual))
            {
                var w = MinValueVisual.GetDesiredWidthIfSet();
                if (w > width)
                {
                    width = w;
                }
            }
            else
            {
                var h = MinValueVisual.GetDesiredHeightIfSet();
                if (h > height)
                {
                    height = h;
                }
            }
        }

        if (MaxValueVisual != null)
        {
            MaxValueVisual.Measure(constraint);
            if (!IsVerticalText(MaxValueVisual))
            {
                var w = MaxValueVisual.GetDesiredWidthIfSet();
                if (w > width)
                {
                    width = w;
                }
            }
            else
            {
                var h = MaxValueVisual.GetDesiredHeightIfSet();
                if (h > height)
                {
                    height = h;
                }
            }
        }
        Width = width;
        Height = height;

        // determine track sizes based on value position
        if (TryConvertToSingle(MaxValue - MinValue, out var range) &&
            range != 0 &&
            TryConvertToSingle(MinValue, out var minValue) &&
            TryConvertToSingle(Value, out var value))
        {
            MinValueVisual?.Measure(constraint);
            MaxValueVisual?.Measure(constraint);
            Thumb?.Measure(constraint);

            float totalTrackSize;
            if (Orientation == Orientation.Horizontal)
            {
                totalTrackSize = constraint.width - MinValueVisual.GetDesiredWidthIfSet() - MaxValueVisual.GetDesiredWidthIfSet() - Thumb.GetDesiredWidthIfSet();
            }
            else
            {
                totalTrackSize = constraint.height - MinValueVisual.GetDesiredHeightIfSet() - MaxValueVisual.GetDesiredHeightIfSet() - Thumb.GetDesiredHeightIfSet();
            }

            var minTrackSize = Math.Max(0, totalTrackSize * (value - minValue) / range);
            var maxTrackSize = Math.Max(0, totalTrackSize - minTrackSize);

            if (Orientation == Orientation.Horizontal)
            {
                MinTrackVisual?.Width = minTrackSize;
                MaxTrackVisual?.Width = maxTrackSize;
                //TicksVisual?.Width = minTrackSize + Thumb.GetDesiredWidthIfSet() + maxTrackSize;
            }
            else
            {
                MinTrackVisual?.Height = minTrackSize;
                MaxTrackVisual?.Height = maxTrackSize;
                //TicksVisual?.Height = minTrackSize + Thumb.GetDesiredHeightIfSet() + maxTrackSize;
            }
        }

        return base.MeasureCore(constraint);
    }

    private void OnValueChanged()
    {
        // avoid oscillating between min and max when they are set to invalid values (stack overflow)
        if (MinValue > MaxValue)
            return;

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

        if (MinValueVisual is TextBox tb)
        {
            tb.Text = GetValueString(SliderValueContext.MinValue, MinValue);
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
            Value = MaxValue;
        }

        if (MaxValue < MinValue)
        {
            MinValue = MaxValue;
        }

        if (MaxValueVisual is IValueable valueable)
        {
            valueable.TrySetValue(MaxValue);
        }

        if (MaxValueVisual is TextBox tb)
        {
            tb.Text = GetValueString(SliderValueContext.MaxValue, MaxValue);
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
    /// </summary>
    /// <param name="sender">Event source (window).</param>
    /// <param name="e">Theme/DPI event args.</param>
    protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        if (!AutoSize)
            return;

        var theme = GetWindowTheme();
        var boxSize = theme.BoxSize;

        //if (TicksVisual != null)
        //{
        //    ClipChildren = false;
        //    ClipFromParent = false;
        //}

        if (Orientation == Orientation.Horizontal)
        {
            //TicksVisual?.Height = boxSize;
            Height = boxSize;
        }
        else
        {
            //TicksVisual?.Width = boxSize;
            Width = boxSize;
        }

        if (Thumb != null)
        {
            Thumb.Height = boxSize;
            Thumb.Width = boxSize * 4;
        }

        if (Thumb is RoundedRectangle rr)
        {
            rr.CornerRadius = new Vector2(theme.RoundedButtonCornerRadius);
        }

        if (MinValueVisual is TextBox minTb)
        {
            if (Orientation == Orientation.Horizontal)
            {
                minTb.Margin = D2D_RECT_F.Thickness(0, 0, theme.SliderPadding, 0);
            }
            else
            {
                minTb.Margin = D2D_RECT_F.Thickness(0, 0, 0, theme.SliderPadding);
            }
        }

        if (MaxValueVisual is TextBox maxTb)
        {
            if (Orientation == Orientation.Horizontal)
            {
                maxTb.Margin = D2D_RECT_F.Thickness(theme.SliderPadding, 0, 0, 0);
            }
            else
            {
                maxTb.Margin = D2D_RECT_F.Thickness(0, theme.SliderPadding, 0, 0);
            }
        }

        if (MinTrackVisual is RoundedRectangle minR)
        {
            minR.CornerRadius = new Vector2(theme.RoundedButtonCornerRadius);

            if (Orientation == Orientation.Horizontal)
            {
                minR.Height = boxSize / 2;
                minR.RenderOffset = new Vector3(theme.RoundedButtonCornerRadius, 0, 0);
            }
            else
            {
                minR.Width = boxSize / 2;
                minR.RenderOffset = new Vector3(0, theme.RoundedButtonCornerRadius, 0);
            }

            // to be consistent with max track
            minR.ZIndex = -1;
        }

        if (MaxTrackVisual is RoundedRectangle maxR)
        {
            maxR.CornerRadius = new Vector2(theme.RoundedButtonCornerRadius);

            // we don't want to see the rounded corner on the top side of the max track,
            // so we offset it to the top by the corner radius and set ZIndex to -1 to render it behind the thumb
            if (Orientation == Orientation.Horizontal)
            {
                maxR.Height = boxSize / 2;
                maxR.RenderOffset = new Vector3(-theme.RoundedButtonCornerRadius, 0, 0);
            }
            else
            {
                maxR.Width = boxSize / 2;
                maxR.RenderOffset = new Vector3(0, -theme.RoundedButtonCornerRadius, 0);
            }

            maxR.ZIndex = -1;
        }
    }

    /// <summary>
    /// Represents a popup window that displays the current value of a slider control.
    /// </summary>
    protected partial class SliderValueWindow : PopupWindow, IContentParent
    {
        private readonly TextBox _text = new();

        /// <summary>
        /// Initializes a new instance of the SliderValueWindow class, which displays the current value of a slider
        /// control in a separate window.
        /// </summary>
        /// <param name="slider">The slider control associated with this window. Cannot be null.</param>
        public SliderValueWindow(BaseSlider<T> slider)
        {
            ArgumentNullException.ThrowIfNull(slider);
            Slider = slider;

            MeasureToContent = DimensionOptions.WidthAndHeight;
            FrameSize = 0;
            ClickThrough = true;
            FollowPlacementTarget = true;
            PlacementTarget = slider.Thumb ?? slider;
            PlacementMode = PlacementMode.Center;
            IsFocusable = false;

            var theme = GetWindowTheme();
            var hsv = Hsv.From(theme.SliderThumbColor);
            hsv.Value *= 1.3f; // lighten the thumb color

            Content = CreateContent();
            if (Content == null)
                throw new InvalidOperationException();

            var shadow = Compositor!.CreateDropShadow();
            shadow.BlurRadius = GetWindowTheme().ToolTipShadowBlurRadius;
            Content.RenderShadow = shadow;
            Content.RenderBrush = Compositor!.CreateColorBrush(hsv.ToD3DCOLORVALUE().ToColor());
            Content.Margin = 0;

            Children.Add(Content);

            var boxSize = theme.BoxSize * 3 / 2;
            _text = new TextBox
            {
                ForegroundBrush = new SolidColorBrush(theme.UnselectedColor),
                Margin = boxSize / 4
            };
            Content.Children.Add(_text);

            Update();
        }

        /// <summary>
        /// Gets the slider control that enables selection of a value within a specified range.
        /// </summary>
        public BaseSlider<T> Slider { get; }

        /// <summary>
        /// Gets the single content visual hosted by the window.
        /// Derived types can override <see cref="CreateContent"/> to customize the visual.
        /// </summary>
        [Browsable(false)]
        public Visual Content { get; }

        /// <inheritdoc/>
        protected override bool HasCaret => false;

        /// <inheritdoc/>
        protected override MA OnMouseActivate(HWND parentWindowHandle, int mouseMessage, HT hitTest) => MA.MA_NOACTIVATE;

        /// <inheritdoc/>
        protected override void OnHandleCreated(object? sender, EventArgs e)
        {
            unsafe
            {
                // works only on Windows 11, does nothing on Windows 10, so we don't check error
                var corner = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUNDSMALL;
                Functions.DwmSetWindowAttribute(Handle, (uint)DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, (nint)(&corner), 4);
            }
        }

        /// <inheritdoc/>
        protected override void ExtendFrame(HWND handle)
        {
            // don't extend frame
            //base.ExtendFrame(handle);
        }

        /// <summary>
        /// Creates the content visual for the tooltip.
        /// The default is a <see cref="Canvas"/> measuring to its content.
        /// </summary>
        /// <returns>The visual to host inside the tooltip; never null.</returns>
        protected virtual Visual CreateContent() => new Canvas { MeasureToContent = DimensionOptions.WidthAndHeight };

        /// <summary>
        /// Updates the displayed text to reflect the current value of the slider.
        /// </summary>
        public virtual void Update() => _text.Text = Slider.GetValueString(SliderValueContext.Unspecified, Slider.Value);

        /// <inheritdoc/>
        protected override PlacementParameters CreatePlacementParameters()
        {
            var parameters = base.CreatePlacementParameters();

            var theme = GetWindowTheme();
            var boxSize = theme.BoxSize * 3 / 2;

            if (Slider.Orientation == Orientation.Horizontal)
            {
                parameters.VerticalOffset = -boxSize;
                parameters.HorizontalOffset = 0;
            }
            else
            {
                parameters.HorizontalOffset = boxSize;
                parameters.VerticalOffset = 0;
            }
            return parameters;
        }
    }
}
