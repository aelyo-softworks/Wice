namespace Wice;

/// <summary>
/// A caption button used in a window title bar (e.g., Close/Maximize/Minimize).
/// Renders a vector <see cref="Path"/> whose geometry is supplied by the current theme/resource manager,
/// sized using DPI-aware caption metrics retrieved from DWM.
/// </summary>
public partial class TitleBarButton : ButtonBase
{
    private const float _strokeThickness = 1;
    private TitleBarButtonType _buttonType;
    private GeometrySource2D? _lastGeometrySource2D;

    /// <summary>
    /// Initializes a new instance of <see cref="TitleBarButton"/> and creates the icon <see cref="Path"/>.
    /// </summary>
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

    /// <summary>
    /// Gets the inner <see cref="Path"/> that draws the button icon.
    /// </summary>
    [Category(CategoryLayout)]
    public Path Path { get; }

    /// <summary>
    /// Gets or sets the semantic type of this title bar button (affects icon geometry).
    /// </summary>
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

    /// <summary>
    /// Creates the <see cref="Path"/> used to render the icon. Override to customize shape defaults.
    /// </summary>
    /// <returns>A new <see cref="Path"/> instance.</returns>
    protected virtual Path CreatePath() => new();

    /// <summary>
    /// Measures the button and sizes the icon <see cref="Path"/> to a square based on caption height.
    /// </summary>
    /// <param name="constraint">Available size including margin.</param>
    /// <returns>The desired size excluding margin.</returns>
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        var window = Window;
        if (window != null)
        {
            // DWM returns the full caption button bounds; we use one third of the height for the glyph itself.
            var dpiSize = GetDpiAdjustedCaptionButtonSize(window).ToD2D_SIZE_F();
            var height = dpiSize.height / 3;
            Path.Height = height;
            Path.Width = height;
        }

        var size = base.MeasureCore(constraint);
        return size;
    }

    /// <summary>
    /// Applies the appropriate geometry to the icon path based on the arranged size and <see cref="ButtonType"/>.
    /// </summary>
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

    /// <summary>
    /// Sets stroke thickness and a default black stroke brush for the icon path when composition is available.
    /// </summary>
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        if (Path.Shape != null)
        {
            Path.Shape.StrokeThickness = _strokeThickness;
        }

        Path.StrokeBrush = Compositor!.CreateColorBrush(D3DCOLORVALUE.Black.ToColor());
    }

    /// <summary>
    /// Returns the DPI-adjusted caption button size (width/height) for the given <paramref name="window"/>.
    /// </summary>
    /// <param name="window">The window whose caption metrics to query.</param>
    /// <returns>A <see cref="SIZE"/> representing a single caption button size.</returns>
    public unsafe static SIZE GetDpiAdjustedCaptionButtonSize(Window window)
    {
        ExceptionExtensions.ThrowIfNull(window, nameof(window));
        var bounds = new RECT();
        var size = (uint)sizeof(RECT);
        WiceCommons.DwmGetWindowAttribute(window.Handle, (uint)DWMWINDOWATTRIBUTE.DWMWA_CAPTION_BUTTON_BOUNDS, (nint)(&bounds), size);
        var height = bounds.Height;

        if (!window.AdaptToDpi)
        {
            height = AdjustForMonitorDpi(window, height, true);
        }

        // Ensure stroke aligns to pixels by making height even when using an odd stroke thickness.
        if (_strokeThickness % 2 == 1 && height % 2 == 1)
        {
            height++;
        }

        // Assume 3 caption buttons; subtract 1px to avoid rounding spill-over and divide.
        var width = (bounds.Width - 1) / 3;

        if (!window.AdaptToDpi)
        {
            width = AdjustForMonitorDpi(window, width, true);
        }
        return new SIZE(width, height);
    }

    private static int AdjustForMonitorDpi(Window window, int value, bool reduce = false)
    {
        var dpi = window.Dpi;
        if (dpi == WiceCommons.USER_DEFAULT_SCREEN_DPI)
            return value;

        if (reduce)
            return (int)(value * WiceCommons.USER_DEFAULT_SCREEN_DPI / dpi);

        return (int)(value * dpi / WiceCommons.USER_DEFAULT_SCREEN_DPI);
    }
}
