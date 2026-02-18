namespace Wice;

/// <summary>
/// Represents a base visual that allows users to select a value from a specified range by sliding a handle along a track.
/// Default behavior requires the number type T to be convertible to System.Single for proper measurement and value calculations,
/// otherwise you should override at least the TryConvertToSingle and TryConvertFromSingle methods.
/// </summary>
/// <typeparam name="T">The type of the value that the slider can represent, constrained to types that implement the INumber interface.</typeparam>
public partial class Slider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T> : Stack, IValueable where T : INumber<T>, IMinMaxValue<T>
{
    /// <summary>
    /// Dynamic property descriptor for <see cref="Value"/>.
    /// </summary>
    public static VisualProperty ValueProperty { get; } = VisualProperty.Add(typeof(Slider<T>), nameof(Value), VisualPropertyInvalidateModes.Measure, T.Zero, changed: OnValueChanged);

    /// <summary>
    /// Dynamic property descriptor for <see cref="KeyboardStep"/>.
    /// Default value is determined by the type T, with floating-point types defaulting to 0.1 and other numeric types defaulting to 1.
    /// </summary>
    public static VisualProperty KeyboardStepProperty { get; } = VisualProperty.Add(typeof(Slider<T>), nameof(KeyboardStep), VisualPropertyInvalidateModes.Render, GetDefaultStepValue());

    /// <summary>
    /// Dynamic property descriptor for <see cref="TicksStep"/>.
    /// Default value is determined by the type T, with floating-point types defaulting to 0.1 and other numeric types defaulting to 1.
    /// </summary>
    public static VisualProperty TicksStepProperty { get; } = VisualProperty.Add(typeof(Slider<T>), nameof(TicksStep), VisualPropertyInvalidateModes.Render, GetDefaultTicksStepValue());

    /// <summary>
    /// Dynamic property descriptor for <see cref="MinValue"/>.
    /// </summary>
    public static VisualProperty MinValueProperty { get; } = VisualProperty.Add(typeof(Slider<T>), nameof(MinValue), VisualPropertyInvalidateModes.Measure, T.Zero, changed: OnMinValueChanged);

    /// <summary>
    /// Dynamic property descriptor for <see cref="MaxValue"/>.
    /// Default value is determined by the type T, with numeric types defaulting to 100 (integer types) or 1 (floating types), and other types defaulting to T.MaxValue.
    /// </summary>
    public static VisualProperty MaxValueProperty { get; } = VisualProperty.Add(typeof(Slider<T>), nameof(MaxValue), VisualPropertyInvalidateModes.Measure, GetDefaultMaxValue(), changed: OnMaxValueChanged);

    /// <summary>
    /// Attached property backing <see cref="Orientation"/>.
    /// </summary>
    // note the name *must* be different than base class name
    public static new VisualProperty OrientationProperty { get; } = VisualProperty.Add(typeof(Slider<T>), nameof(Slider<>) + nameof(Orientation), VisualPropertyInvalidateModes.Measure, Orientation.Horizontal);

    /// <summary>
    /// Attached property backing <see cref="TextOrientation"/>.
    /// </summary>
    public static VisualProperty TextOrientationProperty { get; } = VisualProperty.Add(typeof(Slider<T>), nameof(TextOrientation), VisualPropertyInvalidateModes.Measure, Orientation.Horizontal);

    /// <summary>
    /// Dynamic property descriptor for <see cref="TicksSteps"/>.
    /// </summary>
    public static VisualProperty TicksStepsProperty { get; } = VisualProperty.Add<T[]>(typeof(Slider<T>), nameof(TicksSteps), VisualPropertyInvalidateModes.Render, null, changed: OnTicksStepsChanged);

    /// <summary>
    /// Dynamic property descriptor for <see cref="TicksOptions"/>.
    /// </summary>
    public static VisualProperty TicksOptionsProperty { get; } = VisualProperty.Add(typeof(Slider<T>), nameof(TicksOptions), VisualPropertyInvalidateModes.Measure, SliderTicksOptions.ShowTicks);

    /// <summary>
    /// Dynamic property descriptor for <see cref="SnapToTicks"/>.
    /// </summary>
    public static VisualProperty SnapToTicksProperty { get; } = VisualProperty.Add(typeof(Slider<T>), nameof(SnapToTicks), VisualPropertyInvalidateModes.Render, false, changed: OnSnapToTicksChanged);

    private static void OnValueChanged(BaseObject obj, object? newValue, object? oldValue) => ((Slider<T>)obj).OnValueChanged();
    private static void OnMinValueChanged(BaseObject obj, object? newValue, object? oldValue) => ((Slider<T>)obj).OnMinValueChanged();
    private static void OnMaxValueChanged(BaseObject obj, object? newValue, object? oldValue) => ((Slider<T>)obj).OnMaxValueChanged();
    private static void OnTicksStepsChanged(BaseObject obj, object? newValue, object? oldValue) => ((Slider<T>)obj).OnTicksStepsChanged();
    private static void OnSnapToTicksChanged(BaseObject obj, object? newValue, object? oldValue) => ((Slider<T>)obj).OnSnapToTicksChanged();

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

    private static T GetDefaultTicksStepValue()
    {
        var step = GetDefaultStepValue();
        try
        {
            return T.MultiplyAddEstimate(step, T.CreateChecked(10), T.Zero);
        }
        catch
        {
            return step;
        }
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
    public Slider()
    {
        SetAlignments();
        IsFocusable = true;
        SliderVisual = CreateSliderDock();
        if (SliderVisual != null)
        {
#if DEBUG
            SliderVisual.Name = nameof(SliderVisual);
#endif
            Children.Add(SliderVisual);

            MinValueVisual = CreateMinValueVisual();
            if (MinValueVisual != null)
            {
#if DEBUG
                MinValueVisual.Name = nameof(MinValueVisual);
#endif
                SliderVisual.Children.Add(MinValueVisual);
            }

            MinTrackVisual = CreateMinTrackVisual();
            if (MinTrackVisual != null)
            {
#if DEBUG
                MinTrackVisual.Name = nameof(MinTrackVisual);
#endif
                SliderVisual.Children.Add(MinTrackVisual);
            }

            Thumb = CreateThumb();
            if (Thumb != null)
            {
#if DEBUG
                Thumb.Name = nameof(Thumb);
#endif
                SliderVisual.Children.Add(Thumb);

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
                SliderVisual.Children.Add(MaxValueVisual);
            }

            MaxTrackVisual = CreateMaxTrackVisual();
            if (MaxTrackVisual != null)
            {
#if DEBUG
                MaxTrackVisual.Name = nameof(MaxTrackVisual);
#endif
                SliderVisual.Children.Add(MaxTrackVisual);
            }
        }

        TicksVisual = CreateTicksVisual();
        if (TicksVisual != null)
        {
#if DEBUG
            TicksVisual.Name = nameof(TicksVisual);
#endif
            Children.Add(TicksVisual);
        }

        if (SliderVisual is SliderDock dock)
        {
            dock.SetDockTypesAndAlignments();
        }
    }

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
    /// Gets or sets the slider orientation.
    /// </summary>
    [Category(CategoryLayout)]
    public new Orientation Orientation { get => (Orientation)GetPropertyValue(OrientationProperty)!; set => SetPropertyValue(OrientationProperty, value); }

    /// <summary>
    /// Gets or sets the slider texts orientation.
    /// </summary>
    [Category(CategoryLayout)]
    public Orientation TextOrientation { get => (Orientation)GetPropertyValue(TextOrientationProperty)!; set => SetPropertyValue(TextOrientationProperty, value); }

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
    /// Gets or sets a value indicating whether a window displaying the current value is shown when the visual is
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
    /// Gets or sets the array of steps for the ticks visual.
    /// </summary>
    [Category(CategoryBehavior)]
    public T[]? TicksSteps { get => (T[]?)GetPropertyValue(TicksStepsProperty); set => SetPropertyValue(TicksStepsProperty, value); }

    /// <summary>
    /// Gets or sets the keyboard step value of the visual.
    /// </summary>
    [Category(CategoryBehavior)]
    public T KeyboardStep { get => (T)GetPropertyValue(KeyboardStepProperty)!; set => SetPropertyValue(KeyboardStepProperty, value); }

    /// <summary>
    /// Gets or sets the ticks options of the visual.
    /// </summary>
    [Category(CategoryBehavior)]
    public SliderTicksOptions TicksOptions { get => (SliderTicksOptions)GetPropertyValue(TicksOptionsProperty)!; set => SetPropertyValue(TicksOptionsProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the visual aligns its value to the nearest tick mark on the axis.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool SnapToTicks { get => (bool)GetPropertyValue(SnapToTicksProperty)!; set => SetPropertyValue(SnapToTicksProperty, value); }

    /// <summary>
    /// Gets or sets the step value of the ticks.
    /// </summary>
    [Category(CategoryBehavior)]
    public T TicksStep { get => (T)GetPropertyValue(TicksStepProperty)!; set => SetPropertyValue(TicksStepProperty, value); }

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
    /// Gets the visual representation associated with the slider.
    /// By default, this is the visual that contains all other visuals for the slider, except the ticks visual.
    /// </summary>
    [Browsable(false)]
    public Visual? SliderVisual { get; }

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
    /// Creates a new Tick instance initialized with the specified value.
    /// </summary>
    /// <param name="ticks">The Ticks visual that the Tick instance will be associated with.</param>
    /// <param name="value">The value to initialize the Tick instance with.</param>
    /// <returns>A new Tick instance initialized with the provided value.</returns>
    protected virtual Tick CreateTick(Ticks ticks, T value) => new(ticks, value);

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
    /// Creates a new visual element that represents the slider component for the current context.
    /// </summary>
    /// <returns>A <see cref="SliderDock "/> instance that provides the visual representation of the slider.</returns>
    protected virtual Visual CreateSliderDock() => new SliderDock(this);

    /// <summary>
    /// Creates a visual representation of the minimum value. By default, it's using a TextBox visual.
    /// </summary>
    /// <returns>A visual displaying the minimum value as text.</returns>
    protected virtual Visual CreateMinValueVisual()
    {
        var tb = new TextBox
        {
            Text = GetValueString(SliderValueContext.MinValue, MinValue),
        };
        tb.CopyFrom(this);
        return tb;
    }

    /// <summary>
    /// Creates a visual representation of the minimum track. By default, it's using a RoundedRectangle visual.
    /// </summary>
    /// <returns>A visual displaying the minimum track as text.</returns>
    protected virtual Visual CreateMinTrackVisual()
    {
        var rr = new RoundedRectangle
        {
        };
        return rr;
    }

    /// <summary>
    /// Creates a new instance of the Thumb visual that is configured to handle drag operations and is vertically
    /// centered.
    /// </summary>
    /// <returns>A Thumb visual that responds to drag events and is aligned to the vertical center.</returns>
    protected virtual Visual CreateThumb()
    {
        var th = new Thumb
        {
            IsFocusable = false,
            ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, Value?.ToString() ?? string.Empty)
        };
        return th;
    }

    /// <summary>
    /// Creates a visual representation of the maximum track. By default, it's using a RoundedRectangle visual.
    /// </summary>
    /// <returns>A visual displaying the maximum track as text.</returns>
    protected virtual Visual CreateMaxTrackVisual()
    {
        var rr = new RoundedRectangle
        {
        };
        return rr;
    }

    /// <summary>
    /// Creates a visual representation of the maximum value. By default, it's using a TextBox visual.
    /// </summary>
    /// <returns>A visual displaying the maximum value as text.</returns>
    protected virtual Visual CreateMaxValueVisual()
    {
        var tb = new TextBox
        {
            Text = GetValueString(SliderValueContext.MaxValue, MaxValue),
        };
        tb.CopyFrom(this);
        return tb;
    }

    /// <summary>
    /// Creates a visual representation of the ticks for the slider visual.
    /// </summary>
    /// <returns>A visual representing the ticks.</returns>
    protected virtual Visual CreateTicksVisual()
    {
        var cv = new Ticks(this)
        {
        };
        return cv;
    }

    /// <summary>
    /// Creates a new instance of the SliderValueWindow used to display the current value of the slider.
    /// </summary>
    /// <returns>A new instance of SliderValueWindow that represents the window for displaying slider values.</returns>
    protected virtual SliderValueWindow CreateValueWindow() => new(this);

    /// <summary>
    /// Displays a value window at the appropriate location relative to the visual, using the current orientation and theme
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
    /// Invoked when a drag operation on the thumb visual is completed.
    /// </summary>
    /// <param name="sender">The source of the event, typically the thumb visual that was dragged.</param>
    /// <param name="e">An object that contains the event data.</param>
    protected virtual void OnThumbDragCompleted(object? sender, EventArgs e)
    {
        if (SnapToTicks)
        {
            var steps = GetSteps();
            if (steps.Count <= 1)
                return;

            var nearest = steps.OrderBy(s => T.Abs(s - Value)).FirstOrDefault();
            if (nearest == null)
                return;

            Value = nearest;
        }

        CloseValueWindow();
    }

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
    public virtual bool TryConvertToSingle(T value, out float result) => Conversions.TryChangeType(value, out result);

    /// <summary>
    /// Attempts to convert the specified single-precision floating-point value to the target type.
    /// </summary>
    /// <param name="value">The single-precision floating-point number to convert.</param>
    /// <param name="result">When this method returns, contains the converted value if the conversion succeeded; otherwise, null.</param>
    /// <returns>true if the conversion was successful; otherwise, false.</returns>
    public virtual bool TryConvertFromSingle(float value, [NotNullWhen(true)] out T? result) => Conversions.TryChangeType(value, out result);

    /// <summary>
    /// Adjusts the alignments of the element based on its current orientation. When the orientation is horizontal, sets
    /// the vertical alignment to center and the horizontal alignment to stretch; when vertical, sets the vertical
    /// alignment to stretch and the horizontal alignment to center.
    /// </summary>
    protected virtual void SetAlignments()
    {
        if (Orientation == Orientation.Horizontal)
        {
            VerticalAlignment = Alignment.Center;
            HorizontalAlignment = Alignment.Stretch;
        }
        else
        {
            VerticalAlignment = Alignment.Stretch;
            HorizontalAlignment = Alignment.Center;
        }

        if (MinValueVisual is TextBox min)
        {
            min.Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER;
            min.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;
        }

        if (MaxValueVisual is TextBox max)
        {
            max.Alignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER;
            max.ParagraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;
        }
    }

    /// <summary>
    /// Generates a sequence of steps based on the defined step size and range constraints.
    /// </summary>
    /// <returns>An IReadOnlyList containing the generated steps. If no steps are defined, an empty list is returned.</returns>
    protected virtual IReadOnlyList<T> GetSteps()
    {
        var steps = TicksSteps;
        if (steps != null && steps.Length > 0)
            return steps;

        var step = TicksStep;
        if (T.IsNegative(step) || step.Equals(T.Zero))
            return [MinValue, MaxValue];

        var list = new List<T>();
        for (var t = MinValue; t < MaxValue; t = T.MultiplyAddEstimate(t, T.MultiplicativeIdentity, step))
        {
            list.Add(t);
        }

        list.Add(MaxValue);
        return list;
    }

    /// <inheritdoc/>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        // slider orientation is inverted because the default slider dock layout is vertical, so when orientation is vertical we want to use horizontal layout and vice versa
        if (property == OrientationProperty)
        {
            var orientation = (Orientation)value!;
            base.Orientation = orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
            if (SliderVisual is SliderDock dock)
            {
                dock.SetDockTypesAndAlignments();
            }

            SetAlignments();
            return true;
        }

        if (property == TextOrientationProperty)
        {
            var orientation = (Orientation)value!;
            if (MinValueVisual is TextBox minTb)
            {
                minTb.ReadingDirection = orientation == Orientation.Horizontal ? DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_LEFT_TO_RIGHT : DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_TOP_TO_BOTTOM;
                minTb.FlowDirection = orientation == Orientation.Horizontal ? DWRITE_FLOW_DIRECTION.DWRITE_FLOW_DIRECTION_TOP_TO_BOTTOM : DWRITE_FLOW_DIRECTION.DWRITE_FLOW_DIRECTION_RIGHT_TO_LEFT;
            }

            if (MaxValueVisual is TextBox maxTb)
            {
                maxTb.ReadingDirection = orientation == Orientation.Horizontal ? DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_LEFT_TO_RIGHT : DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_TOP_TO_BOTTOM;
                maxTb.FlowDirection = orientation == Orientation.Horizontal ? DWRITE_FLOW_DIRECTION.DWRITE_FLOW_DIRECTION_TOP_TO_BOTTOM : DWRITE_FLOW_DIRECTION.DWRITE_FLOW_DIRECTION_RIGHT_TO_LEFT;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    protected override void OnMouseButtonDown(object? sender, MouseButtonEventArgs e)
    {
        base.OnMouseButtonDown(sender, e);

        if (!TryConvertToSingle(MaxValue - MinValue, out var range) || range == 0)
            return;

        var theme = GetWindowTheme();
        float totalLength;
        float pos;
        if (Orientation == Orientation.Horizontal)
        {
            pos = e.GetPosition(this).x - MinValueVisual.GetArrangedWidthIfSet() - theme.SliderPadding - theme.RoundedButtonCornerRadius;

            var w = SliderVisual.GetDesiredWidthIfSet() - MinValueVisual.GetDesiredWidthIfSet() - MaxValueVisual.GetDesiredWidthIfSet();
            totalLength = w - theme.SliderPadding * 2 - theme.RoundedButtonCornerRadius * 2;
        }
        else
        {
            pos = e.GetPosition(this).y - MinValueVisual.GetArrangedHeightIfSet() - theme.SliderPadding - theme.RoundedButtonCornerRadius;

            var h = SliderVisual.GetDesiredHeightIfSet() - MinValueVisual.GetDesiredHeightIfSet() - MaxValueVisual.GetDesiredHeightIfSet();
            totalLength = h - theme.SliderPadding * 2 - theme.RoundedButtonCornerRadius * 2;
        }
        if (totalLength <= 0)
            return;

        var ratio = pos * range / totalLength;
        if (!TryConvertFromSingle(ratio, out var value))
            return;

        Value = T.MultiplyAddEstimate(value, T.MultiplicativeIdentity, MinValue);
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsEnabled || !IsFocused)
            return;

        var step = KeyboardStep;
        if (!SnapToTicks)
        {
            var ctrl = NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_CONTROL);
            if (ctrl && TryConvertFromSingle(LargeStepFactor, out var large))
            {
                step *= large;
            }
        }

        switch (e.Key)
        {
            case VIRTUAL_KEY.VK_LEFT:
            case VIRTUAL_KEY.VK_UP:
                if (SnapToTicks)
                {
                    var steps = GetSteps();
                    var s = steps.Where(s => s < Value).OrderDescending().ToArray();
                    var nearest = steps.Where(s => s < Value).OrderDescending().FirstOrDefault();
                    if (nearest == null) // probably zero
                        break;

                    if (nearest > Value)
                        break;

                    Value = nearest;
                }
                else
                {
                    Value -= step;
                }
                e.Handled = true;
                break;

            case VIRTUAL_KEY.VK_RIGHT:
            case VIRTUAL_KEY.VK_DOWN:
                if (SnapToTicks)
                {
                    var steps = GetSteps();
                    var nearest = steps.Where(s => s > Value).Order().FirstOrDefault();
                    if (nearest == null) // probably zero
                        break;

                    if (nearest < Value)
                        break;

                    Value = nearest;
                }
                else
                {
                    Value += step;
                }
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

    /// <inheritdoc />
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        var size = base.MeasureCore(constraint);
        if (IsTicksVisible() && SliderVisual != null)
        {
            var theme = GetWindowTheme();
            if (Orientation == Orientation.Horizontal)
            {
                var w = SliderVisual.GetDesiredWidthIfSet() - MinValueVisual.GetDesiredWidthIfSet() - MaxValueVisual.GetDesiredWidthIfSet();
                TicksVisual.Width = Math.Max(0, w - theme.SliderPadding * 2 - theme.RoundedButtonCornerRadius * 2);
            }
            else
            {
                var h = SliderVisual.GetDesiredHeightIfSet() - MinValueVisual.GetDesiredHeightIfSet() - MaxValueVisual.GetDesiredHeightIfSet();
                TicksVisual.Height = Math.Max(0, h - theme.SliderPadding * 2 - theme.RoundedButtonCornerRadius * 2);
            }

            if (TicksVisual is Ticks ticks)
            {
                ticks.Update();
            }
        }
        return size;
    }

    /// <inheritdoc />
    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        base.ArrangeCore(finalRect);
        if (IsTicksVisible())
        {
            var theme = GetWindowTheme();
            var arr = TicksVisual.ArrangedRect;
            if (Orientation == Orientation.Horizontal)
            {
                var ar = MinValueVisual.GetArrangedRightIfSet();
                arr = new D2D_RECT_F(ar + theme.SliderPadding + theme.RoundedButtonCornerRadius, arr.top, ar + arr.Width, arr.bottom);
            }
            else
            {
                var ab = MinValueVisual.GetArrangedBottomIfSet();
                arr = new D2D_RECT_F(arr.left, ab + theme.SliderPadding + theme.RoundedButtonCornerRadius, arr.right, ab + arr.Height);
            }

            if (arr.IsValid)
            {
                TicksVisual.Arrange(arr);
            }
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

    private void OnSnapToTicksChanged()
    {
        if (!SnapToTicks)
            return;

        var min = T.MaxValue;
        var max = T.MinValue;
        foreach (var value in GetSteps())
        {
            if (value > max)
            {
                max = value;
            }

            if (value < min)
            {
                min = value;
            }
        }

        if (Value < min)
        {
            Value = min;
            return;
        }

        if (Value > max)
        {
            Value = max;
            return;
        }
    }

    private void OnTicksStepsChanged()
    {
        var newValue = TicksSteps;
        if (newValue == null || newValue.Length == 0)
            return;

        var min = T.MaxValue;
        var max = T.MinValue;
        foreach (var value in newValue)
        {
            if (value < MinValue || value > MaxValue)
                throw new WiceException($"0038: TicksSteps array contains a value ({value}) that is not between MinValue ({MinValue}) and MaxValue ({MaxValue}).");

            if (value > max)
            {
                max = value;
            }

            if (value < min)
            {
                min = value;
            }
        }

        if (SnapToTicks)
        {
            if (Value < min)
            {
                Value = min;
            }

            if (Value > max)
            {
                Value = max;
            }
        }
    }

    [MemberNotNullWhen(true, nameof(TicksVisual))]
    private bool IsTicksVisible()
    {
        if (TicksVisual == null)
            return false;

        var steps = GetSteps();
        if (steps.Count < 2)
            return false;

        if (TicksOptions == SliderTicksOptions.None)
            return false;

        return true;
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

    private float MeasureText(string text, float fontSize)
    {
        var theme = GetWindowTheme();
        var format = Application.CurrentResourceManager.GetTextFormat(theme,
            null,
            fontSize,
            DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER,
            DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER
            )!;

        using var layout = Application.CurrentResourceManager.CreateTextLayout(format, text);
        var metrics = layout.GetMetrics1();
        return metrics.Base.width;
    }

    /// <summary>
    /// Applies automatic sizing based on current theme metrics when <see cref="AutoSize"/> is true.
    /// </summary>
    /// <param name="sender">Event source (window).</param>
    /// <param name="e">Theme/DPI event args.</param>
    protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        var theme = GetWindowTheme();
        var boxSize = theme.BoxSize;
        var ticksSize = 0f;
        if (IsTicksVisible())
        {
            if (TicksOptions.HasFlag(SliderTicksOptions.ShowTickValues))
            {
                // If the text orientation is different from the slider orientation, we measure the text size and add padding.
                // We assume the MaxValue text is the largest.
                if (TextOrientation != Orientation)
                {
                    // we add a small padding after the text to avoid the text being too close to the tick's edge
                    ticksSize = MeasureText(GetValueString(SliderValueContext.MaxValue, MaxValue), theme.SliderTickValueFontSize) + theme.SliderPadding / 2;
                }
                else
                {
                    ticksSize += theme.SliderTickValueFontSize;
                }
                ticksSize += theme.SliderPadding;
            }

            if (TicksOptions.HasFlag(SliderTicksOptions.ShowTicks))
            {
                ticksSize += theme.SliderTickSize + theme.SliderPadding;
            }
        }

        if (Orientation == Orientation.Horizontal)
        {
            if (Height.IsNotSet() && VerticalAlignment != Alignment.Stretch)
            {
                Height = boxSize + ticksSize;
                SliderVisual?.Height = boxSize;
            }
        }
        else
        {
            if (Width.IsNotSet() && HorizontalAlignment != Alignment.Stretch)
            {
                Width = boxSize + ticksSize;
                SliderVisual?.Width = boxSize;
            }
        }

        if (Thumb != null)
        {
            if (Thumb.Height.IsNotSet())
            {
                Thumb.Height = boxSize;
            }

            if (Thumb.Width.IsNotSet())
            {
                Thumb.Width = boxSize;
            }
        }

        if (AutoSize)
        {
            if (Thumb is RoundedRectangle rr)
            {
                rr.CornerRadius = new Vector2(theme.RoundedButtonCornerRadius);
            }

            if (MinValueVisual != null)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    MinValueVisual.Margin = D2D_RECT_F.Thickness(0, 0, theme.SliderPadding, 0);
                }
                else
                {
                    MinValueVisual.Margin = D2D_RECT_F.Thickness(0, 0, 0, theme.SliderPadding);
                }
            }

            if (MaxValueVisual != null)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    MaxValueVisual.Margin = D2D_RECT_F.Thickness(theme.SliderPadding, 0, 0, 0);
                }
                else
                {
                    MaxValueVisual.Margin = D2D_RECT_F.Thickness(0, theme.SliderPadding, 0, 0);
                }
            }

            if (MinTrackVisual is RoundedRectangle minR)
            {
                minR.CornerRadius = new Vector2(theme.RoundedButtonCornerRadius);
            }

            if (MinTrackVisual != null)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    MinTrackVisual.Height = boxSize / 2;
                    MinTrackVisual.RenderOffset = new Vector3(theme.RoundedButtonCornerRadius, 0, 0);
                }
                else
                {
                    MinTrackVisual.Width = boxSize / 2;
                    MinTrackVisual.RenderOffset = new Vector3(0, theme.RoundedButtonCornerRadius, 0);
                }

                // to be consistent with max track
                MinTrackVisual.ZIndex = -1;
            }

            if (MaxTrackVisual is RoundedRectangle maxR)
            {
                maxR.CornerRadius = new Vector2(theme.RoundedButtonCornerRadius);
            }

            if (MaxTrackVisual != null)
            {

                // we don't want to see the rounded corner on the top side of the max track,
                // so we offset it to the top by the corner radius and set ZIndex to -1 to render it behind the thumb
                if (Orientation == Orientation.Horizontal)
                {
                    MaxTrackVisual.Height = boxSize / 2;
                    MaxTrackVisual.RenderOffset = new Vector3(-theme.RoundedButtonCornerRadius, 0, 0);
                }
                else
                {
                    MaxTrackVisual.Width = boxSize / 2;
                    MaxTrackVisual.RenderOffset = new Vector3(0, -theme.RoundedButtonCornerRadius, 0);
                }

                MaxTrackVisual.ZIndex = -1;
            }
        }

        if (MinTrackVisual != null)
        {
            if (Orientation == Orientation.Horizontal)
            {
                if (MinTrackVisual.Height.IsNotSet())
                {
                    MinTrackVisual.Height = boxSize / 2;
                }
            }
            else
            {
                if (MinTrackVisual.Width.IsNotSet())
                {
                    MinTrackVisual.Width = boxSize / 2;
                }
            }
        }

        if (MaxTrackVisual != null)
        {
            if (Orientation == Orientation.Horizontal)
            {
                if (MaxTrackVisual.Height.IsNotSet())
                {
                    MaxTrackVisual.Height = boxSize / 2;
                }
            }
            else
            {
                if (MaxTrackVisual.Width.IsNotSet())
                {
                    MaxTrackVisual.Width = boxSize / 2;
                }
            }
        }

        if (TicksVisual is Ticks ticks)
        {
            ticks.OnThemeDpiEvent(sender, e);
        }
    }

    /// <summary>
    /// Represents a visual that displays a tick for the slider.
    /// </summary>
    protected partial class Tick : Stack
    {
        /// <summary>
        /// Initializes a new instance of the Ticks class with the specified slider.
        /// </summary>
        /// <param name="ticks">The Ticks visual that this tick belongs to, providing context for the tick's position and value within the slider.</param>
        /// <param name="value">The value associated with this tick, which will be displayed as a visual reference on the slider.</param>
        public Tick(Ticks ticks, T value)
        {
            ArgumentNullException.ThrowIfNull(ticks);

            Ticks = ticks;
            Value = value;

            // stack orientation is inverted from slider
            if (Slider.Orientation == Orientation.Horizontal)
            {
                Orientation = Orientation.Vertical;
                HorizontalAlignment = Alignment.Center;
            }
            else
            {
                Orientation = Orientation.Horizontal;
                VerticalAlignment = Alignment.Center;
            }

            if (Slider.TicksOptions.HasFlag(SliderTicksOptions.ShowTicks))
            {
                TickVisual = CreateTickVisual();
                if (TickVisual != null)
                {
                    Children.Add(TickVisual);
                }
            }

            if (Slider.TicksOptions.HasFlag(SliderTicksOptions.ShowTickValues))
            {
                ValueVisual = CreateValueVisual();
                if (ValueVisual != null)
                {
                    Children.Add(ValueVisual);
                }
            }

            UpdateStyle();
        }

        /// <summary>
        /// Gets the number of ticks that represent the time interval.
        /// </summary>
        public Ticks Ticks { get; }

        /// <summary>
        /// Gets the slider visual that enables selection of a value within a specified range.
        /// </summary>
        public Slider<T> Slider => Ticks.Slider;

        /// <summary>
        /// Gets the value stored in the current instance.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets the visual representation used to display the tick mark in the visual.
        /// </summary>
        public Visual? TickVisual { get; }

        /// <summary>
        /// Gets the visual element that represents the current value of the slider.
        /// </summary>
        public Visual? ValueVisual { get; }

        /// <inheritdoc/>
        public override string ToString() => Value?.ToString() ?? string.Empty;

        /// <summary>
        /// Creates a visual element that represents a tick mark for the slider visual.
        /// </summary>
        /// <remarks>Override this method in a derived class to provide a custom visual representation for
        /// tick marks.</remarks>
        /// <returns>A <see cref="Visual"/> object that represents the visual appearance of a tick mark.</returns>
        protected virtual Visual CreateTickVisual()
        {
            var rr = new RoundedRectangle
            {
            };
            return rr;
        }

        /// <summary>
        /// Creates a visual element that displays the current value of the slider.
        /// </summary>
        /// <returns>A <see cref="Visual"/> object that shows the formatted value of the slider.</returns>
        protected virtual Visual CreateValueVisual()
        {
            var tb = new TextBox
            {
                Text = Slider.GetValueString(SliderValueContext.Tick, Value),
            };
            tb.CopyFrom(Slider);
            return tb;
        }

        /// <summary>
        /// Updates the visual style of the slider's value display and tick marks to match the current window theme.
        /// </summary>
        public virtual void UpdateStyle()
        {
            var theme = GetWindowTheme();
            if (ValueVisual is TextBox tb)
            {
                tb.FontSize = theme.SliderTickValueFontSize;

                tb.ReadingDirection = Slider.TextOrientation == Orientation.Horizontal
                    ? DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_LEFT_TO_RIGHT
                    : DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_TOP_TO_BOTTOM;

                tb.FlowDirection = Slider.TextOrientation == Orientation.Horizontal ?
                    DWRITE_FLOW_DIRECTION.DWRITE_FLOW_DIRECTION_TOP_TO_BOTTOM :
                    DWRITE_FLOW_DIRECTION.DWRITE_FLOW_DIRECTION_RIGHT_TO_LEFT;
            }

            if (TickVisual is RoundedRectangle rr)
            {
                if (Slider.Orientation == Orientation.Horizontal)
                {
                    rr.Width = theme.SliderTickThickness;
                    rr.Height = theme.SliderTickSize;
                }
                else
                {
                    rr.Width = theme.SliderTickSize;
                    rr.Height = theme.SliderTickThickness;
                }

                var radius = theme.RoundedButtonCornerRadius / 2;
                rr.CornerRadius = new Vector2(radius, radius);
            }
        }

        /// <inheritdoc/>
        protected override void Render()
        {
            base.Render();

            var theme = GetWindowTheme();
            if (TickVisual is Shape ticks)
            {
                ticks.FillBrush = Compositor!.CreateColorBrush(theme.SliderThumbColor.ToColor());
            }
        }
    }

    /// <summary>
    /// Represents a visual that displays ticks for the slider, providing a visual reference for the values along the slider's range.
    /// </summary>
    protected partial class Ticks : Canvas
    {
        private Orientation Orientation => Slider.Orientation;
        private T? _minValue;
        private T? _maxValue;
        private SliderTicksOptions _options;

        /// <summary>
        /// Initializes a new instance of the Ticks class with the specified slider.
        /// </summary>
        /// <param name="slider">The slider for which the ticks are being created.</param>
        public Ticks(Slider<T> slider)
        {
            ArgumentNullException.ThrowIfNull(slider);
            Slider = slider;

            // to be able to show max value tick
            ClipChildren = false;
            ClipFromParent = false;
        }

        /// <summary>
        /// Gets the slider visual that enables selection of a value within a specified range.
        /// </summary>
        public Slider<T> Slider { get; }

        /// <summary>
        /// Applies automatic sizing based on current theme metrics when <see cref="AutoSize"/> is true.
        /// </summary>
        /// <param name="sender">Event source (window).</param>
        /// <param name="e">Theme/DPI event args.</param>
        public virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
        {
            foreach (var tick in Children.OfType<Tick>())
            {
                tick.UpdateStyle();
            }
        }

        /// <summary>
        /// Updates the position of the specified tick based on the current orientation and size of the slider.
        /// </summary>
        /// <param name="tick">The tick to position within the slider. Represents a specific value on the slider's scale.</param>
        /// <param name="range">The total range of values represented by the slider. Used to calculate the proportional position of the
        /// tick.</param>
        protected virtual void UpdateTickPosition(Tick tick, float range)
        {
            if (!Slider.TryConvertToSingle(tick.Value - Slider.MinValue, out var stepValue))
                return;

            if (Orientation == Orientation.Horizontal)
            {
                if (Width.IsNotSet())
                    return;
            }
            else
            {
                if (Height.IsNotSet())
                    return;
            }

            var theme = GetWindowTheme();
            if (Orientation == Orientation.Horizontal)
            {
                SetLeft(tick, stepValue * (Width - theme.SliderPadding - theme.RoundedButtonCornerRadius) / range);
                SetTop(tick, theme.SliderPadding);
            }
            else
            {
                SetTop(tick, stepValue * (Height - theme.SliderPadding - theme.RoundedButtonCornerRadius) / range);
                SetLeft(tick, theme.SliderPadding);
            }
        }

        /// <summary>
        /// Updates visual to reflect the current value of the slider.
        /// </summary>
        public virtual void Update()
        {
            if (!Slider.TryConvertToSingle(Slider.MaxValue - Slider.MinValue, out var range))
                return;

            if (_minValue == Slider.MinValue && _maxValue == Slider.MaxValue && _options == Slider.TicksOptions)
            {
                foreach (var tick in Children.OfType<Tick>())
                {
                    UpdateTickPosition(tick, range);
                    tick.UpdateStyle();
                }
                return;
            }

            Children.Clear();

            foreach (var step in Slider.GetSteps())
            {
                var tick = Slider.CreateTick(this, step);
                if (tick == null)
                    continue;

                Children.Add(tick);
            }

            _minValue = Slider.MinValue;
            _maxValue = Slider.MaxValue;
            _options = Slider.TicksOptions;
        }
    }

    /// <summary>
    /// Represents a visual that integrates a slider for selecting a value within a specified range.
    /// </summary>
    protected partial class SliderDock : Dock
    {
        private Orientation Orientation => Slider.Orientation;

        /// <summary>
        /// Initializes a new instance of the SliderDock class with the specified slider.
        /// </summary>
        /// <param name="slider">The slider visual that this dock will contain and manage.</param>
        public SliderDock(Slider<T> slider)
        {
            ArgumentNullException.ThrowIfNull(slider);
            Slider = slider;
        }

        /// <summary>
        /// Gets the slider visual that enables selection of a value within a specified range.
        /// </summary>
        public Slider<T> Slider { get; }

        /// <summary>
        /// Configures the docking positions of the slider's visual components based on the current orientation.
        /// </summary>
        public virtual void SetDockTypesAndAlignments()
        {
            if (Orientation == Orientation.Horizontal)
            {
                if (Slider.MinValueVisual != null)
                {
                    SetDockType(Slider.MinValueVisual, DockType.Left);
                    Slider.MinValueVisual.HorizontalAlignment = Alignment.Near;
                    Slider.MinValueVisual.VerticalAlignment = Alignment.Center;
                }

                if (Slider.MinTrackVisual != null)
                {
                    SetDockType(Slider.MinTrackVisual, DockType.Left);
                    Slider.MinTrackVisual.HorizontalAlignment = Alignment.Near;
                    Slider.MinTrackVisual.VerticalAlignment = Alignment.Center;
                }

                if (Slider.Thumb != null)
                {
                    SetDockType(Slider.Thumb, DockType.Left);
                    Slider.Thumb.HorizontalAlignment = Alignment.Near;
                    Slider.Thumb.VerticalAlignment = Alignment.Center;
                }

                if (Slider.MaxValueVisual != null)
                {
                    SetDockType(Slider.MaxValueVisual, DockType.Right);
                    Slider.MaxValueVisual.HorizontalAlignment = Alignment.Far;
                    Slider.MaxValueVisual.VerticalAlignment = Alignment.Center;
                }

                if (Slider.MaxTrackVisual != null)
                {
                    SetDockType(Slider.MaxTrackVisual, DockType.Right);
                    Slider.MaxTrackVisual.HorizontalAlignment = Alignment.Far;
                    Slider.MaxTrackVisual.VerticalAlignment = Alignment.Center;
                }
            }
            else
            {
                if (Slider.MinValueVisual != null)
                {
                    SetDockType(Slider.MinValueVisual, DockType.Top);
                    Slider.MinValueVisual.HorizontalAlignment = Alignment.Center;
                    Slider.MinValueVisual.VerticalAlignment = Alignment.Near;
                }

                if (Slider.MinTrackVisual != null)
                {
                    SetDockType(Slider.MinTrackVisual, DockType.Top);
                    Slider.MinTrackVisual.HorizontalAlignment = Alignment.Center;
                    Slider.MinTrackVisual.VerticalAlignment = Alignment.Near;
                }

                if (Slider.Thumb != null)
                {
                    SetDockType(Slider.Thumb, DockType.Top);
                    Slider.Thumb.HorizontalAlignment = Alignment.Center;
                    Slider.Thumb.VerticalAlignment = Alignment.Near;
                }

                if (Slider.MaxValueVisual != null)
                {
                    SetDockType(Slider.MaxValueVisual, DockType.Bottom);
                    Slider.MaxValueVisual.HorizontalAlignment = Alignment.Center;
                    Slider.MaxValueVisual.VerticalAlignment = Alignment.Far;
                }

                if (Slider.MaxTrackVisual != null)
                {
                    SetDockType(Slider.MaxTrackVisual, DockType.Bottom);
                    Slider.MaxTrackVisual.HorizontalAlignment = Alignment.Center;
                    Slider.MaxTrackVisual.VerticalAlignment = Alignment.Far;
                }
            }
        }

        /// <inheritdoc />
        protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
        {
            // handle texts verticality to properly measure width and height, since vertical text would swap those
            var width = Width;
            var height = Height;
            if (Slider.MinValueVisual != null)
            {
                Slider.MinValueVisual.Measure(constraint);
                if (!IsVerticalText(Slider.MinValueVisual))
                {
                    var w = Slider.MinValueVisual.GetDesiredWidthIfSet();
                    if (w > width)
                    {
                        width = w;
                    }
                }
                else
                {
                    var h = Slider.MinValueVisual.GetDesiredHeightIfSet();
                    if (h > height)
                    {
                        height = h;
                    }
                }
            }

            if (Slider.MaxValueVisual != null)
            {
                Slider.MaxValueVisual.Measure(constraint);
                if (!IsVerticalText(Slider.MaxValueVisual))
                {
                    var w = Slider.MaxValueVisual.GetDesiredWidthIfSet();
                    if (w > width)
                    {
                        width = w;
                    }
                }
                else
                {
                    var h = Slider.MaxValueVisual.GetDesiredHeightIfSet();
                    if (h > height)
                    {
                        height = h;
                    }
                }
            }
            Width = width;
            Height = height;

            // determine track sizes based on value position
            if (Slider.TryConvertToSingle(Slider.MaxValue - Slider.MinValue, out var range) &&
                range != 0 &&
                Slider.TryConvertToSingle(Slider.MinValue, out var minValue) &&
                Slider.TryConvertToSingle(Slider.Value, out var value))
            {
                Slider.MinValueVisual?.Measure(constraint);
                Slider.MaxValueVisual?.Measure(constraint);
                Slider.Thumb?.Measure(constraint);

                float totalTrackSize;
                if (Orientation == Orientation.Horizontal)
                {
                    totalTrackSize = constraint.width - Slider.MinValueVisual.GetDesiredWidthIfSet() - Slider.MaxValueVisual.GetDesiredWidthIfSet() - Slider.Thumb.GetDesiredWidthIfSet();
                }
                else
                {
                    totalTrackSize = constraint.height - Slider.MinValueVisual.GetDesiredHeightIfSet() - Slider.MaxValueVisual.GetDesiredHeightIfSet() - Slider.Thumb.GetDesiredHeightIfSet();
                }

                if (totalTrackSize.IsSet())
                {
                    var minTrackSize = Math.Max(0, totalTrackSize * (value - minValue) / range);
                    var maxTrackSize = Math.Max(0, totalTrackSize - minTrackSize);

                    if (Orientation == Orientation.Horizontal)
                    {
                        Slider.MinTrackVisual?.Width = minTrackSize;
                        Slider.MaxTrackVisual?.Width = maxTrackSize;
                    }
                    else
                    {
                        Slider.MinTrackVisual?.Height = minTrackSize;
                        Slider.MaxTrackVisual?.Height = maxTrackSize;
                    }
                }
            }

            return base.MeasureCore(constraint);
        }
    }

    /// <summary>
    /// Represents a popup window that displays the current value of a slider visual.
    /// </summary>
    protected partial class SliderValueWindow : PopupWindow, IContentParent
    {
        private readonly TextBox _text;

        /// <summary>
        /// Initializes a new instance of the SliderValueWindow class, which displays the current value of a slider
        /// visual in a separate window.
        /// </summary>
        /// <param name="slider">The slider visual associated with this window. Cannot be null.</param>
        public SliderValueWindow(Slider<T> slider)
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
            _text.CopyFrom(Slider);
            Content.Children.Add(_text);

            Update();
        }

        /// <summary>
        /// Gets the slider visual that enables selection of a value within a specified range.
        /// </summary>
        public Slider<T> Slider { get; }

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
        /// Updates window to reflect the current value of the slider.
        /// </summary>
        public virtual void Update()
        {
            _text.Text = Slider.GetValueString(SliderValueContext.Unspecified, Slider.Value);

            _text.ReadingDirection = Slider.TextOrientation == Orientation.Horizontal
                ? DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_LEFT_TO_RIGHT
                : DWRITE_READING_DIRECTION.DWRITE_READING_DIRECTION_TOP_TO_BOTTOM;

            _text.FlowDirection = Slider.TextOrientation == Orientation.Horizontal ?
                DWRITE_FLOW_DIRECTION.DWRITE_FLOW_DIRECTION_TOP_TO_BOTTOM :
                DWRITE_FLOW_DIRECTION.DWRITE_FLOW_DIRECTION_RIGHT_TO_LEFT;
        }

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
