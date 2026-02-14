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
            return (T)(object).1f;

        if (typeof(T) == typeof(double))
            return (T)(object).1d;

        if (typeof(T) == typeof(decimal))
            return (T)(object).1m;

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

    private ToolTip? _tt;
    private TextBox? _ttText;
    private Timer? _hideKeyTooltip;

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
            if (orientation == Orientation.Horizontal)
            {
                SetDockType(MinValueVisual, DockType.Left);
            }
            else
            {
                SetDockType(MinValueVisual, DockType.Top);
            }

            Children.Add(MinValueVisual);
        }

        MinTrackVisual = CreateMinTrackVisual();
        if (MinTrackVisual != null)
        {
#if DEBUG
            MinTrackVisual.Name = nameof(MinTrackVisual);
#endif
            if (orientation == Orientation.Horizontal)
            {
                SetDockType(MinTrackVisual, DockType.Left);
            }
            else
            {
                SetDockType(MinTrackVisual, DockType.Top);
            }

            Children.Add(MinTrackVisual);
        }

        Thumb = CreateThumb();
        if (Thumb != null)
        {
#if DEBUG
            Thumb.Name = nameof(Thumb);
#endif
            if (orientation == Orientation.Horizontal)
            {
                SetDockType(Thumb, DockType.Left);
            }
            else
            {
                SetDockType(Thumb, DockType.Top);
            }

            Children.Add(Thumb);

            if (Thumb is IThumb th)
            {
                th.DragDelta += OnThumbDragDelta;
                th.DragStarted += OnThumbDragStarted;
                th.DragCompleted += OnThumbDragCompleted;
            }

            Thumb.IsFocusable = false;
            Thumb.ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, Value?.ToString() ?? string.Empty);
        }

        MaxValueVisual = CreateMaxValueVisual();
        if (MaxValueVisual != null)
        {
#if DEBUG
            MaxValueVisual.Name = nameof(MaxValueVisual);
#endif
            if (orientation == Orientation.Horizontal)
            {
                SetDockType(MaxValueVisual, DockType.Right);
            }
            else
            {
                SetDockType(MaxValueVisual, DockType.Bottom);
            }

            Children.Add(MaxValueVisual);
        }

        MaxTrackVisual = CreateMaxTrackVisual();
        if (MaxTrackVisual != null)
        {
#if DEBUG
            MaxTrackVisual.Name = nameof(MaxTrackVisual);
#endif
            if (orientation == Orientation.Horizontal)
            {
                SetDockType(MaxTrackVisual, DockType.Right);
            }
            else
            {
                SetDockType(MaxTrackVisual, DockType.Bottom);
            }

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

    /// <summary>
    /// Gets or sets the duration, in milliseconds, before a tooltip is automatically hidden.
    /// </summary>
    [Category(CategoryBehavior)]
    public int ToolTipHideTimeout { get; set; } = 2000;

    /// <summary>
    /// Gets or sets the format string that determines how the value is converted to its string representation.
    /// </summary>
    [Category(CategoryBehavior)]
    public string? ValueStringFormat { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a tooltip displaying the current value is shown when the control is
    /// hovered over.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual bool EnableValueToolTip { get; set; } = true;

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
            HorizontalAlignment = Alignment.Center,
            VerticalAlignment = Alignment.Center,
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
            HorizontalAlignment = Alignment.Center,
            VerticalAlignment = Alignment.Center,
        };
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
            HorizontalAlignment = Alignment.Center,
            VerticalAlignment = Alignment.Center,
        };

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
            HorizontalAlignment = Alignment.Center,
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
            Text = GetValueString(SliderValueContext.MaxValue, MaxValue),
            HorizontalAlignment = Alignment.Center,
            VerticalAlignment = Alignment.Center,
            Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER,
            ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER,

            // vertical text would be configured like this
            //ReadingDirection = DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_TOP_TO_BOTTOM,
            //FlowDirection = DWRITE_FLOW_DIRECTION.DWRITE_FLOW_DIRECTION_RIGHT_TO_LEFT
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

    /// <summary>
    /// Displays a tooltip at the appropriate location relative to the control, using the current orientation and theme
    /// settings.
    /// </summary>
    protected virtual void ShowToolTip()
    {
        if (_tt != null)
            return;

        _tt = new ToolTip
        {
            FollowPlacementTarget = true,
            PlacementTarget = Thumb ?? this,
            PlacementMode = PlacementMode.Center,
            IsFocusable = false
        };

        var theme = GetWindowTheme();
        var boxSize = theme.BoxSize * 3 / 2;
        if (Orientation == Orientation.Horizontal)
        {
            _tt.VerticalOffset = -boxSize;
            _tt.HorizontalOffset = 0;
        }
        else
        {
            _tt.HorizontalOffset = boxSize;
            _tt.VerticalOffset = 0;
        }

        CreateToolTipContent(_tt);
        _tt.Show();
    }

    /// <summary>
    /// Updates the tooltip text to display the current value of the slider.
    /// </summary>
    protected virtual void UpdateToolTip()
    {
        _ttText?.Text = GetValueString(SliderValueContext.ToolTip, Value);
    }

    /// <summary>
    /// Closes the currently displayed tooltip and releases any associated resources.
    /// </summary>
    protected virtual void CloseToolTip()
    {
        _tt?.Close();
        _tt = null;
        _ttText = null;
    }

    /// <summary>
    /// Creates and customizes the content displayed in the specified tooltip instance.
    /// </summary>
    protected virtual void CreateToolTipContent(ToolTip tt)
    {
        if (tt == null)
            return;

        // note we must use the tooltip's compositor, not our window's compositor
        // as the tooltip has its own window
        var theme = GetWindowTheme();
        var hsv = Hsv.From(theme.SliderThumbColor);
        hsv.Value *= 1.3f; // lighten the thumb color
        tt.Content.RenderBrush = tt.Compositor!.CreateColorBrush(hsv.ToD3DCOLORVALUE().ToColor());

        var boxSize = theme.BoxSize * 3 / 2;
        _ttText = new TextBox
        {
            Text = GetValueString(SliderValueContext.Unspecified, Value),
            ForegroundBrush = new SolidColorBrush(D3DCOLORVALUE.White),
            Margin = boxSize / 4
        };
        tt.Content.Children.Add(_ttText);
    }

    /// <inheritdoc/>
    protected internal override void IsFocusedChanged(bool newValue)
    {
        base.IsFocusedChanged(newValue);
        if (!newValue)
        {
            _hideKeyTooltip?.Dispose();
            _hideKeyTooltip = null;
        }
    }

    /// <summary>
    /// Invoked when a drag operation on the thumb control is completed.
    /// </summary>
    /// <param name="sender">The source of the event, typically the thumb control that was dragged.</param>
    /// <param name="e">An object that contains the event data.</param>
    protected virtual void OnThumbDragCompleted(object? sender, EventArgs e) => CloseToolTip();

    /// <summary>
    /// Invoked when a drag operation on the thumb visual begins, allowing derived classes to handle the start of the
    /// drag event.
    /// </summary>
    /// <param name="sender">The source of the event, typically the thumb visual that initiated the drag operation.</param>
    /// <param name="e">A DragEventArgs object that contains the event data for the drag operation.</param>
    protected virtual void OnThumbDragStarted(object? sender, DragEventArgs e)
    {
        _hideKeyTooltip?.Dispose();
        Focus();
        if (!TryConvertToSingle(Value - MinValue, out var value))
            return;

        e.State.Tag = value;
        if (EnableValueToolTip)
        {
            ShowToolTip();
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

        if (!TryConvertToSingle(MaxValue - MinValue, out var range))
            return;

        float trackSize;
        if (Orientation == Orientation.Horizontal)
        {
            var minValueSize = MinValueVisual?.ArrangedRect.Width ?? 0;
            var maxValueSize = MaxValueVisual?.ArrangedRect.Width ?? 0;
            trackSize = ArrangedRect.Width - minValueSize - maxValueSize;
        }
        else
        {
            var minValueSize = MinValueVisual?.ArrangedRect.Height ?? 0;
            var maxValueSize = MaxValueVisual?.ArrangedRect.Height ?? 0;
            trackSize = ArrangedRect.Height - minValueSize - maxValueSize;
        }

        var deltaPos = Orientation == Orientation.Horizontal ? e.State.DeltaX : e.State.DeltaY;
        var newValue = startValue + deltaPos * range / trackSize;

        if (!TryConvertFromSingle(newValue, out var newValueT))
            return;

        Value = MinValue + newValueT;
    }

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

        if (e.Handled)
        {
            if (EnableValueToolTip)
            {
                ShowToolTip();
                _hideKeyTooltip?.Dispose();
                if (ToolTipHideTimeout > 0)
                {
                    _hideKeyTooltip = new Timer(_ =>
                    {
                        CloseToolTip();
                        _hideKeyTooltip?.Dispose();
                        _hideKeyTooltip = null;
                    }, null, ToolTipHideTimeout, Timeout.Infinite);
                }
            }
        }
        base.OnKeyDown(sender, e);
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
        UpdateToolTip();
    }

    private static bool IsVerticalText(Visual visual)
    {
        if (visual is TextBox tb)
            return tb.ReadingDirection is DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_TOP_TO_BOTTOM or DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_BOTTOM_TO_TOP;
        return false;
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
                if (MinValueVisual.DesiredSize.width.IsSet() && MinValueVisual.DesiredSize.width > width)
                {
                    width = MinValueVisual.DesiredSize.width;
                }
            }
            else
            {
                if (MinValueVisual.DesiredSize.height.IsSet() && MinValueVisual.DesiredSize.height > height)
                {
                    height = MinValueVisual.DesiredSize.height;
                }
            }
        }

        if (MaxValueVisual != null)
        {
            MaxValueVisual.Measure(constraint);
            if (!IsVerticalText(MaxValueVisual))
            {
                if (MaxValueVisual.DesiredSize.width.IsSet() && MaxValueVisual.DesiredSize.width > width)
                {
                    width = MaxValueVisual.DesiredSize.width;
                }
            }
            else
            {
                if (MaxValueVisual.DesiredSize.height.IsSet() && MaxValueVisual.DesiredSize.height > height)
                {
                    height = MaxValueVisual.DesiredSize.height;
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
                totalTrackSize = constraint.width - (MinValueVisual?.DesiredSize.width ?? 0) - (MaxValueVisual?.DesiredSize.width ?? 0) - (Thumb?.DesiredSize.width ?? 0);
            }
            else
            {
                totalTrackSize = constraint.height - (MinValueVisual?.DesiredSize.height ?? 0) - (MaxValueVisual?.DesiredSize.height ?? 0) - (Thumb?.DesiredSize.height ?? 0);
            }

            var minTrackSize = Math.Max(0, totalTrackSize * (value - minValue) / range);
            var maxTrackSize = Math.Max(0, totalTrackSize - minTrackSize);

            if (Orientation == Orientation.Horizontal)
            {
                MinTrackVisual?.Width = minTrackSize;
                MaxTrackVisual?.Width = maxTrackSize;
            }
            else
            {
                MinTrackVisual?.Height = minTrackSize;
                MaxTrackVisual?.Height = maxTrackSize;
            }
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

        if (Orientation == Orientation.Horizontal)
        {
            Height = boxSize;
        }
        else
        {
            Width = boxSize;
        }

        if (Thumb != null)
        {
            Thumb.Height = boxSize;
            Thumb.Width = boxSize;
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
}
