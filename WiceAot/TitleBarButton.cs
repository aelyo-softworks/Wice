namespace Wice;

public partial class TitleBarButton : ButtonBase
{
    private const float _strokeThickness = 1;

    private TitleBarButtonType _buttonType;
    private GeometrySource2D? _lastGeometrySource2D;

    public TitleBarButton()
    {
        Path = CreatePath();
        if (Path == null)
            throw new InvalidOperationException();

#if DEBUG
        Path.Name = nameof(Path);
#endif
        Children.Add(Path);
    }

    [Category(CategoryLayout)]
    public Path Path { get; }

    [Category(CategoryBehavior)]
    public TitleBarButtonType ButtonType
    {
        get => _buttonType;
        set
        {
            if (_buttonType == value)
                return;

            _buttonType = value;
            Path.Name = nameof(Path) + _buttonType;
        }
    }

    protected virtual Path CreatePath() => new();

    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        var window = Window;
        if (window != null)
        {
            var dpiSize = GetDpiAdjustedCaptionButtonSize(window).ToD2D_SIZE_F();
            var height = dpiSize.height / 3;
            Path.Height = height;
            Path.Width = height;
        }

        var size = base.MeasureCore(constraint);
        return size;
    }

    protected override void OnArranged(object? sender, EventArgs e)
    {
        base.OnArranged(sender, e);
        var size = Path.ArrangedRect;
        var geoSize = size.Height;
        var geoSource = Application.CurrentResourceManager.GetTitleBarButtonGeometrySource(ButtonType, geoSize);
        if (geoSource.Equals(_lastGeometrySource2D))
            return;

        Path.GeometrySource2D = geoSource;
        _lastGeometrySource2D = geoSource;
    }

    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        if (Path.Shape != null)
        {
            Path.Shape.StrokeThickness = _strokeThickness;
        }

        Path.StrokeBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Black.ToColor());
    }

    // this is dpi-adjusted but only when called *after* some message like SHOWWINDOW or NCPAINT (not sure)
    public unsafe static SIZE GetDpiAdjustedCaptionButtonSize(Window window)
    {
        ExceptionExtensions.ThrowIfNull(window, nameof(window));
        var bounds = new RECT();
        var size = (uint)sizeof(RECT);
        WiceCommons.DwmGetWindowAttribute(window.Handle, (uint)DWMWINDOWATTRIBUTE.DWMWA_CAPTION_BUTTON_BOUNDS, (nint)(&bounds), size);
        var height = bounds.Height;
        height = AdjustForMonitorDpi(window, height, true);
        if (_strokeThickness % 2 == 1 && height % 2 == 1)
        {
            height++;
        }

        // we have 3 buttons. not sure this is always ok...
        var width = (bounds.Width - 1) / 3;
        width = AdjustForMonitorDpi(window, width, true);
        return new SIZE(width, height);
    }

    private static int AdjustForMonitorDpi(Window window, int value, bool reduce = false)
    {
        var dpi = window.Monitor?.EffectiveDpi.width;
        if (!dpi.HasValue)
        {
            dpi = window.Dpi;
        }

        if (dpi == 96)
            return value;

        if (reduce)
            return (int)(value * 96 / dpi.Value);

        return (int)(value * dpi.Value / 96);
    }
}
