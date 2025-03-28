namespace Wice;

// serves for cols and rows
public abstract class GridDimension : BaseObject
{
    private float? _desiredSize;

    public static BaseObjectProperty StarsProperty { get; } = BaseObjectProperty.Add(typeof(GridDimension), nameof(Stars), 1f, convert: ValidateStars);
    public static BaseObjectProperty SizeProperty { get; } = BaseObjectProperty.Add(typeof(GridDimension), nameof(Size), 0f, convert: ValidateSize); // TODO: float.nan?
    public static BaseObjectProperty MinSizeProperty { get; } = BaseObjectProperty.Add(typeof(GridDimension), nameof(MinSize), 0f, convert: ValidateMinMaxSize);
    public static BaseObjectProperty MaxSizeProperty { get; } = BaseObjectProperty.Add(typeof(GridDimension), nameof(MaxSize), float.MaxValue, convert: ValidateMinMaxSize);
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

    protected GridDimension()
    {
    }

    [Browsable(false)]
    public Grid? Parent { get; internal set; }

    [Browsable(false)]
    public Window? Window => Parent?.Window;

    [Category(Visual.CategoryBehavior)]
    public abstract int Index { get; }

    [Browsable(false)]
    public abstract GridDimension? Next { get; }

    [Browsable(false)]
    public abstract GridDimension? Previous { get; }

    [Category(Visual.CategoryBehavior)]
    public bool HasDefinedSize => !HasStarSize && !HasAutoSize;

    [Category(Visual.CategoryBehavior)]
    public bool HasAutoSize => float.IsNaN(Size);

    [Category(Visual.CategoryBehavior)]
    public bool HasStarSize { get { var stars = Stars; return stars.IsValid() && stars != 0; } }

    // like WPF GridLength's stars
    [Category(Visual.CategoryBehavior)]
    public float Stars { get => (float)GetPropertyValue(StarsProperty)!; set => SetPropertyValue(StarsProperty, value); }

    // = width for cols or height for rows
    // NaN means stretch (like WPF's Auto)
    [Category(Visual.CategoryBehavior)]
    public float Size { get => (float)GetPropertyValue(SizeProperty)!; set => SetPropertyValue(SizeProperty, value); }

    [Category(Visual.CategoryBehavior)]
    public float MinSize { get => (float)GetPropertyValue(MinSizeProperty)!; set => SetPropertyValue(MinSizeProperty, value); }

    [Category(Visual.CategoryBehavior)]
    public float MaxSize { get => (float)GetPropertyValue(MaxSizeProperty)!; set => SetPropertyValue(MaxSizeProperty, value); }

    [Category(Visual.CategoryBehavior)]
    public Alignment? DefaultAlignment { get => (Alignment?)GetPropertyValue(DefaultAlignmentProperty); set => SetPropertyValue(DefaultAlignmentProperty, value); }

    [Category(Visual.CategoryBehavior)]
    public float? FinalStartPosition { get; internal set; }

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
