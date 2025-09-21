namespace Wice;

/// <summary>
/// Base dimension descriptor used by <see cref="Grid"/> for both columns and rows.
/// Encapsulates sizing modes:
/// - Fixed: <see cref="Size"/> is a finite value.
/// - Auto: <see cref="Size"/> is <see cref="float.NaN"/> (length is determined by children).
/// - Star: <see cref="Stars"/> &gt; 0 (proportional sizing relative to other star dimensions).
/// </summary>
public abstract class GridDimension : BaseObject
{
    private float? _desiredSize;

    /// <summary>
    /// Backing property for <see cref="Stars"/>. Validates non-negative and finite values.
    /// </summary>
    public static BaseObjectProperty StarsProperty { get; } = BaseObjectProperty.Add(typeof(GridDimension), nameof(Stars), 1f, convert: ValidateStars);

    /// <summary>
    /// Backing property for <see cref="Size"/>. Validates non-negative values and disallows <see cref="float.PositiveInfinity"/>/<see cref="float.NegativeInfinity"/>.
    /// Allows <see cref="float.NaN"/> to represent Auto.
    /// </summary>
    public static BaseObjectProperty SizeProperty { get; } = BaseObjectProperty.Add(typeof(GridDimension), nameof(Size), 0f, convert: ValidateSize); // TODO: float.nan?

    /// <summary>
    /// Backing property for <see cref="MinSize"/>. Validates non-negative, finite values.
    /// </summary>
    public static BaseObjectProperty MinSizeProperty { get; } = BaseObjectProperty.Add(typeof(GridDimension), nameof(MinSize), 0f, convert: ValidateMinMaxSize);

    /// <summary>
    /// Backing property for <see cref="MaxSize"/>. Validates non-negative, finite values.
    /// </summary>
    public static BaseObjectProperty MaxSizeProperty { get; } = BaseObjectProperty.Add(typeof(GridDimension), nameof(MaxSize), float.MaxValue, convert: ValidateMinMaxSize);

    /// <summary>
    /// Backing property for <see cref="DefaultAlignment"/> applied to children when not explicitly set.
    /// </summary>
    public static BaseObjectProperty DefaultAlignmentProperty { get; } = BaseObjectProperty.Add(typeof(GridDimension), nameof(DefaultAlignment), typeof(Alignment?), null, null, null, null);

    private static object? ValidateStars(BaseObject obj, object? value)
    {
        var f = (float)value!;
        if (!f.IsValid() || f < 0f)
            throw new ArgumentOutOfRangeException(nameof(value));

        return f;
    }

    private static object? ValidateSize(BaseObject obj, object? value)
    {
        var f = (float)value!;
        if (float.IsInfinity(f) || f < 0f)
            throw new ArgumentOutOfRangeException(nameof(value));

        return f;
    }

    private static object? ValidateMinMaxSize(BaseObject obj, object? value)
    {
        var f = (float)value!;
        if (!f.IsValid() || f < 0f)
            throw new ArgumentOutOfRangeException(nameof(value));

        return f;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GridDimension"/> class.
    /// </summary>
    protected GridDimension()
    {
    }

    /// <summary>
    /// Gets the owning <see cref="Grid"/>, if any.
    /// </summary>
    [Browsable(false)]
    public Grid? Parent { get; internal set; }

    /// <summary>
    /// Gets the <see cref="Window"/> that contains this dimension via its parent grid, if any.
    /// </summary>
    [Browsable(false)]
    public Window? Window => Parent?.Window;

    /// <summary>
    /// Gets the zero-based index of this dimension within its parent collection.
    /// </summary>
    [Category(Visual.CategoryBehavior)]
    public abstract int Index { get; }

    /// <summary>
    /// Gets the next dimension in the parent collection, or null if this is the last element.
    /// </summary>
    [Browsable(false)]
    public abstract GridDimension? Next { get; }

    /// <summary>
    /// Gets the previous dimension in the parent collection, or null if this is the first element.
    /// </summary>
    [Browsable(false)]
    public abstract GridDimension? Previous { get; }

    /// <summary>
    /// Gets a value indicating whether the dimension has an explicit fixed size
    /// (i.e., not star-sized and not auto-sized).
    /// </summary>
    [Category(Visual.CategoryBehavior)]
    public bool HasDefinedSize => !HasStarSize && !HasAutoSize;

    /// <summary>
    /// Gets a value indicating whether the dimension is auto-sized (i.e., <see cref="Size"/> is <see cref="float.NaN"/>).
    /// Auto sizing typically stretches to fit content.
    /// </summary>
    [Category(Visual.CategoryBehavior)]
    public bool HasAutoSize => float.IsNaN(Size);

    /// <summary>
    /// Gets a value indicating whether the dimension uses star sizing.
    /// Star sizing divides remaining space proportionally among star-sized rows/columns.
    /// </summary>
    [Category(Visual.CategoryBehavior)]
    public bool HasStarSize { get { var stars = Stars; return stars.IsValid() && stars != 0; } }

    /// <summary>
    /// Gets or sets the star factor for proportional sizing (like WPF's GridLength star).
    /// A value of 0 disables star sizing. When set, <see cref="Size"/> is reset to 0.
    /// </summary>
    [Category(Visual.CategoryBehavior)]
    public float Stars { get => (float)GetPropertyValue(StarsProperty)!; set => SetPropertyValue(StarsProperty, value); }

    /// <summary>
    /// Gets or sets the fixed size (width for columns, height for rows).
    /// Use <see cref="float.NaN"/> for Auto (stretch by content). When set, <see cref="Stars"/> is reset to 0.
    /// </summary>
    [Category(Visual.CategoryBehavior)]
    public float Size { get => (float)GetPropertyValue(SizeProperty)!; set => SetPropertyValue(SizeProperty, value); }

    /// <summary>
    /// Gets or sets the minimum size constraint applied to this dimension.
    /// </summary>
    [Category(Visual.CategoryBehavior)]
    public float MinSize { get => (float)GetPropertyValue(MinSizeProperty)!; set => SetPropertyValue(MinSizeProperty, value); }

    /// <summary>
    /// Gets or sets the maximum size constraint applied to this dimension.
    /// </summary>
    [Category(Visual.CategoryBehavior)]
    public float MaxSize { get => (float)GetPropertyValue(MaxSizeProperty)!; set => SetPropertyValue(MaxSizeProperty, value); }

    /// <summary>
    /// Gets or sets the default child alignment used when a child does not specify its own alignment.
    /// </summary>
    [Category(Visual.CategoryBehavior)]
    public Alignment? DefaultAlignment { get => (Alignment?)GetPropertyValue(DefaultAlignmentProperty); set => SetPropertyValue(DefaultAlignmentProperty, value); }

    /// <summary>
    /// Gets the resolved start position for this dimension within the arranged grid, if available.
    /// </summary>
    [Category(Visual.CategoryBehavior)]
    public float? FinalStartPosition { get; internal set; }

    /// <summary>
    /// Gets the internally computed desired size for the dimension, clamped to <see cref="MinSize"/> and <see cref="MaxSize"/>.
    /// </summary>
    [Category(Visual.CategoryBehavior)]
    public float? DesiredSize
    {
        get => _desiredSize;
        internal set
        {
            if (_desiredSize == value)
                return;

            _desiredSize = value;
            if (_desiredSize.HasValue)
            {
                var s = MaxSize;
                if (s.IsSet() && _desiredSize.Value > s)
                {
                    _desiredSize = s;
                }

                s = MinSize;
                if (s.IsSet() && _desiredSize.Value < s)
                {
                    _desiredSize = s;
                }
            }
        }
    }

    /// <summary>
    /// Gets the resolved end position (start + size) within the arranged grid, if available.
    /// </summary>
    [Category(Visual.CategoryBehavior)]
    public float? FinalEndPosition
    {
        get
        {
            if (DesiredSize.HasValue && FinalStartPosition.HasValue)
                return FinalStartPosition.Value + DesiredSize.Value;

            return null;
        }
    }

    /// <summary>
    /// Gets the final arranged size (<see cref="FinalEndPosition"/> - <see cref="FinalStartPosition"/>) if available.
    /// </summary>
    [Category(Visual.CategoryBehavior)]
    public float? FinalSize
    {
        get
        {
            if (!FinalStartPosition.HasValue)
                return null;

            var pos = FinalEndPosition;
            if (!pos.HasValue)
                return null;

            return pos.Value - FinalStartPosition.Value;
        }
    }

    /// <summary>
    /// Overrides property setting to enforce mutual exclusivity between <see cref="Stars"/> and <see cref="Size"/>,
    /// and to invalidate the parent grid for a new measure pass.
    /// </summary>
    /// <param name="property">The property being updated.</param>
    /// <param name="value">The new value.</param>
    /// <param name="options">Optional set options.</param>
    /// <returns>true if the stored value changed; otherwise false.</returns>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        // use base. as we don't want to loop until everyone gets to 0...
        if (property == StarsProperty)
        {
            base.SetPropertyValue(SizeProperty, 0f);
        }
        else if (property == SizeProperty)
        {
            base.SetPropertyValue(StarsProperty, 0f);
        }

        var grid = Parent;
        grid?.Invalidate(VisualPropertyInvalidateModes.Measure, new PropertyInvalidateReason(property));
        return true;
    }

    /// <summary>
    /// Returns a human-readable representation, e.g.:
    /// "[1] 2*" for star-sized, "[0] Auto" for auto-sized, "[2] Fixed: 100" for fixed.
    /// </summary>
    public override string ToString()
    {
        string idx;
        try
        {
            idx = Index.ToString();
        }
        catch
        {
            idx = "-1";
        }

        var s = "[" + idx + "] ";
        if (Stars != 0)
        {
            // all stars have same size
            s += Stars + "*";
        }
        else
        {
            if (HasAutoSize)
            {
                // means set by children
                s += "Auto";
            }
            else
            {
                s += "Fixed: " + Size;
            }
        }
        return s;
    }
}
