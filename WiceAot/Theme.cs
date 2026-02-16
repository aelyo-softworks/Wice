namespace Wice;

/// <summary>
/// Represents a customizable theme for UI elements, including fonts, colors, sizes, and other visual properties.
/// </summary>
public class Theme
{
    /// <summary>
    /// Default Windows symbol font used for modern glyphs (Segoe Fluent/MDL2 Assets).
    /// </summary>
    public const string DefaultSymbolFontName = "Segoe MDL2 Assets";

    /// <summary>
    /// Legacy symbol font used as a fallback on older systems.
    /// </summary>
    public const string DefaultLegacySymbolFontName = "Segoe UI Symbol";

    /// <summary>
    /// Default UI font family name used when no other family is specified.
    /// </summary>
    public const string DefaultDefaultFontFamilyName = "Segoe UI";

    private static Theme _default = new();

    /// <summary>
    /// Gets or sets the global default theme used by windows and visuals that do not specify a custom instance.
    /// </summary>
    public static Theme Default
    {
        get => _default;
        set
        {
            ExceptionExtensions.ThrowIfNull(value, nameof(value));
            _default = value;
        }
    }

    private string? _symbolFontName;
    private string? _legacySymbolFontName;
    private string? _defaultFontFamilyName;
    private float _defaultFontSize;
    private int _defaultRichTextFontSize;
    private float _defaultSplitterSize;
    private readonly Window? _window;

    /// <summary>
    /// Raised when the effective DPI for this theme changes.
    /// </summary>
    public event EventHandler<ThemeDpiEventArgs>? DpiChanged;

    /// <summary>
    /// Creates a theme instance bound to a specific <see cref="Window"/> for DPI updates.
    /// </summary>
    /// <param name="window">The owning window. Must not be null.</param>
    public Theme(Window window)
        : this()
    {
        ExceptionExtensions.ThrowIfNull(window, nameof(window));
        _window = window;
    }

    private Theme()
    {
        ToolTipVisibleTime = ToolTipInitialTime * 10;
        ToolTipReshowTime = ToolTipInitialTime / 5;
        DialogBoxButtonFontSize = DefaultFontSize;
        ListBoxHoverColor = SelectedColor;
    }

    /// <summary>
    /// Default rich text font size when no explicit value is set, in points.
    /// </summary>
    public const int DefaultDefaultRichTextFontSize = 10; // from experience...

    /// <summary>
    /// Gets the default font size used when <see cref="DefaultFontSize"/> is not set (&lt;= 0).
    /// </summary>
    public float DefaultDefaultFontSize { get; protected set; } = 14;

    /// <summary>
    /// Gets the default splitter size used when <see cref="DefaultSplitterSize"/> is not set (&lt;= 0).
    /// </summary>
    public float DefaultDefaultSplitterSize { get; protected set; } = 5;

    /// <summary>
    /// Gets the current DPI this theme is scaled for. 96 represents 100% scaling.
    /// </summary>
    public uint CurrentDpi { get; private set; } = WiceCommons.USER_DEFAULT_SCREEN_DPI;

    /// <summary>
    /// Gets or sets the legacy symbol font family name. When null or empty, falls back to <see cref="DefaultLegacySymbolFontName"/>.
    /// </summary>
    public virtual string? LegacySymbolFontName { get => _legacySymbolFontName.Nullify() ?? DefaultLegacySymbolFontName; set => _legacySymbolFontName = value; }

    /// <summary>
    /// Gets or sets the symbol font family name. When null or empty, falls back to <see cref="DefaultSymbolFontName"/>.
    /// </summary>
    public virtual string? SymbolFontName { get => _symbolFontName.Nullify() ?? DefaultSymbolFontName; set => _symbolFontName = value; }

    /// <summary>
    /// Gets or sets the default font family name. When null or empty, falls back to <see cref="DefaultDefaultFontFamilyName"/>.
    /// </summary>
    public virtual string? DefaultFontFamilyName { get => _defaultFontFamilyName.Nullify() ?? DefaultDefaultFontFamilyName; set => _defaultFontFamilyName = value; }

    /// <summary>
    /// Gets or sets the default font size in points. When not set (&lt;= 0), returns <see cref="DefaultDefaultFontSize"/>.
    /// </summary>
    public virtual float DefaultFontSize { get => _defaultFontSize <= 0 ? DefaultDefaultFontSize : _defaultFontSize; set => _defaultFontSize = value; }

    /// <summary>
    /// Gets or sets the default rich text font size in points. When not set (&lt;= 0), returns <see cref="DefaultDefaultRichTextFontSize"/>.
    /// </summary>
    public virtual int DefaultRichTextFontSize { get => _defaultRichTextFontSize <= 0 ? DefaultDefaultRichTextFontSize : _defaultRichTextFontSize; set => _defaultRichTextFontSize = value; }

    /// <summary>
    /// Gets or sets the default splitter thickness in device-independent pixels. When not set (&lt;= 0), returns <see cref="DefaultDefaultSplitterSize"/>.
    /// </summary>
    public virtual float DefaultSplitterSize { get => _defaultSplitterSize <= 0 ? DefaultDefaultSplitterSize : _defaultSplitterSize; set => _defaultSplitterSize = value; }

    /// <summary>
    /// Gets or sets the default link color.
    /// </summary>
    public virtual D3DCOLORVALUE LinkColor { get; set; }

    /// <summary>
    /// Gets or sets the link color when hovered.
    /// </summary>
    public virtual D3DCOLORVALUE HoverLinkColor { get; set; }

    /// <summary>
    /// Gets or sets the splitter color.
    /// </summary>
    public virtual D3DCOLORVALUE SplitterColor { get; set; } = ColorUtilities.GetSysColor(SYS_COLOR_INDEX.COLOR_ACTIVEBORDER);

    /// <summary>
    /// Gets or sets the border color used for outlines and focus rectangles by default.
    /// </summary>
    public virtual D3DCOLORVALUE BorderColor { get; set; } = D3DCOLORVALUE.Black;

    /// <summary>
    /// Gets or sets the default unselected background color.
    /// </summary>
    public virtual D3DCOLORVALUE UnselectedColor { get; set; } = D3DCOLORVALUE.White;

    /// <summary>
    /// Gets or sets the selected background color (default: blue).
    /// </summary>
    public virtual D3DCOLORVALUE SelectedColor { get; set; } = new D3DCOLORVALUE(0xFF0078D7); // blue

    /// <summary>
    /// Gets or sets the default border thickness in DIPs.
    /// </summary>
    public virtual float BorderSize { get; set; } = 4;

    /// <summary>
    /// Gets or sets the default size for toggle/check visuals or generic boxes in DIPs.
    /// </summary>
    public virtual float BoxSize { get; set; } = 20;

    /// <summary>
    /// Gets or sets the relative thickness (ratio) for toggle borders, applied against <see cref="BoxSize"/>.
    /// </summary>
    public virtual float ToggleBorderRatio { get; set; } = 0.07f;

    /// <summary>
    /// Gets or sets the relative corner radius (ratio) for toggle visuals, applied against <see cref="BoxSize"/>.
    /// </summary>
    public virtual float ToggleRadiusRatio { get; set; } = 0.4f;

    /// <summary>
    /// Gets or sets the opacity ratio applied to disabled elements (0..1).
    /// </summary>
    public virtual float DisabledOpacityRatio { get; set; } = 0.5f;

    /// <summary>
    /// Gets or sets the default list box item background color.
    /// </summary>
    public virtual D3DCOLORVALUE ListBoxItemColor { get; set; } = D3DCOLORVALUE.White;

    /// <summary>
    /// Gets or sets the list box hover background color.
    /// </summary>
    public virtual D3DCOLORVALUE ListBoxHoverColor { get; set; }

    /// <summary>
    /// Gets or sets the color used to render focus adorners.
    /// </summary>
    public virtual D3DCOLORVALUE FocusColor { get; set; } = D3DCOLORVALUE.Black;

    /// <summary>
    /// Gets or sets the focus adorner offset (negative values draw inward).
    /// </summary>
    public virtual float FocusOffset { get; set; } = -2;

    /// <summary>
    /// Gets or sets the focus adorner line thickness.
    /// </summary>
    public virtual float FocusThickness { get; set; } = 1;

    /// <summary>
    /// Gets or sets the dash pattern used for focus adorners.
    /// </summary>
    public virtual float[] FocusDashArray { get; set; } = [2];

    /// <summary>
    /// Gets or sets the default button background color.
    /// </summary>
    public virtual D3DCOLORVALUE ButtonColor { get; set; } = new D3DCOLORVALUE(0xFFCCCCCC);

    /// <summary>
    /// Gets or sets the default margin around buttons (uniform).
    /// </summary>
    public virtual float ButtonMargin { get; set; } = 6;

    /// <summary>
    /// Gets or sets the minimum width for dialog buttons.
    /// </summary>
    public virtual float ButtonMinWidth { get; set; } = 70;

    /// <summary>
    /// Gets or sets the dialog box button font size (in points).
    /// </summary>
    public virtual float DialogBoxButtonFontSize { get; set; }

    /// <summary>
    /// Gets or sets the corner radius for rounded buttons.
    /// </summary>
    public virtual float RoundedButtonCornerRadius { get; set; } = 4;

    /// <summary>
    /// Gets or sets the padding used by message boxes.
    /// </summary>
    public virtual float MessageBoxPadding { get; set; } = 5;

    /// <summary>
    /// Gets or sets the padding around state button lists.
    /// </summary>
    public virtual float StateButtonListPadding { get; set; } = 5;

#if !NETFRAMEWORK
    /// <summary>
    /// Gets or sets the padding around sliders.
    /// </summary>
    public virtual float SliderPadding { get; set; } = 5;

    /// <summary>
    /// Gets or sets the sliders tick thickness.
    /// </summary>
    public virtual float SliderTickThickness { get; set; } = 1;

    /// <summary>
    /// Gets or sets the sliders tick size.
    /// </summary>
    public virtual float SliderTickSize { get; set; } = 12;

    /// <summary>
    /// Gets or sets the slider near track color.
    /// </summary>
    public virtual D3DCOLORVALUE SliderNearColor { get; set; } = ColorUtilities.GetSysColor(SYS_COLOR_INDEX.COLOR_GRADIENTACTIVECAPTION);

    /// <summary>
    /// Gets or sets the slider far track color.
    /// </summary>
    public virtual D3DCOLORVALUE SliderFarColor { get; set; } = ColorUtilities.GetSysColor(SYS_COLOR_INDEX.COLOR_GRADIENTACTIVECAPTION).ChangeAlpha(.2f);

    /// <summary>
    /// Gets or sets the slider thumb color.
    /// </summary>
    public virtual D3DCOLORVALUE SliderThumbColor { get; set; } = new D3DCOLORVALUE(0xFF0078D7); // blue;

    /// <summary>
    /// Gets the default slider font size for tick values.
    /// </summary>
    public float SliderTickValueFontSize { get; protected set; } = 10;

#endif

    /// <summary>
    /// Gets or sets the default caret color for editable visuals.
    /// </summary>
    public virtual D3DCOLORVALUE CaretColor { get; set; } = D3DCOLORVALUE.Blue;

    /// <summary>
    /// Gets or sets the scrollbar button stroke thickness.
    /// </summary>
    public virtual float ScrollBarButtonStrokeThickness { get; set; } = 1;

    /// <summary>
    /// Gets or sets the base background color for scrollbars.
    /// </summary>
    public virtual D3DCOLORVALUE ScrollBarBackgroundColor { get; set; } = new D3DCOLORVALUE(0xFFE9E9E9);

    /// <summary>
    /// Gets or sets the background color for overlay (thin) scrollbars.
    /// </summary>
    public virtual D3DCOLORVALUE ScrollBarOverlayBackgroundColor { get; set; } = new D3DCOLORVALUE(0x40E9E9E9);

    /// <summary>
    /// Gets or sets the stroke color for scrollbar buttons.
    /// </summary>
    public virtual D3DCOLORVALUE ScrollBarButtonStrokeColor { get; set; } = new D3DCOLORVALUE(0xFF969696);

    /// <summary>
    /// Gets or sets the hover color for scrollbar buttons.
    /// </summary>
    public virtual D3DCOLORVALUE ScrollBarButtonHoverColor { get; set; } = new D3DCOLORVALUE(0xFFC9C9C9);

    /// <summary>
    /// Gets or sets the default scrollbar thumb color.
    /// </summary>
    public virtual D3DCOLORVALUE ScrollBarThumbColor { get; set; } = new D3DCOLORVALUE(0xFFC9C9C9);

    /// <summary>
    /// Gets or sets the overlay scrollbar thumb color (higher contrast).
    /// </summary>
    public virtual D3DCOLORVALUE ScrollBarOverlayThumbColor { get; set; } = new D3DCOLORVALUE(0xA0393939);

    /// <summary>
    /// Gets or sets the margin around scrollbar arrows.
    /// </summary>
    public virtual int ScrollBarArrowMargin { get; set; } = 4;

    /// <summary>
    /// Gets or sets the overlay scrollbar corner radius.
    /// </summary>
    public virtual int ScrollBarOverlayCornerRadius { get; set; } = 5;

    /// <summary>
    /// Gets or sets the default text foreground color for text boxes.
    /// </summary>
    public virtual D3DCOLORVALUE TextBoxForegroundColor { get; set; } = ColorUtilities.GetSysColor(SYS_COLOR_INDEX.COLOR_WINDOWTEXT);

    /// <summary>
    /// Gets or sets the selection foreground color for text boxes.
    /// </summary>
    public virtual D3DCOLORVALUE TextBoxSelectionColor { get; set; } = ColorUtilities.GetSysColor(SYS_COLOR_INDEX.COLOR_HIGHLIGHTTEXT);

    /// <summary>
    /// Gets or sets the duration of the selection brush color animation.
    /// </summary>
    public virtual TimeSpan SelectionBrushAnimationDuration { get; set; } = TimeSpan.FromSeconds(0.3f);

    /// <summary>
    /// Gets or sets the default duration used by brush color animations.
    /// </summary>
    public virtual TimeSpan BrushAnimationDuration { get; set; } = TimeSpan.FromSeconds(0.3f);

    /// <summary>
    /// Gets or sets the duration of selected state animations.
    /// </summary>
    public virtual TimeSpan SelectedAnimationDuration { get; set; } = TimeSpan.FromSeconds(0.3f);

    /// <summary>
    /// Gets or sets the tooltip background color.
    /// </summary>
    public virtual D3DCOLORVALUE ToolTipColor { get; set; } = new D3DCOLORVALUE(0xFFFFFFE1);

    /// <summary>
    /// Gets or sets the base font size for tooltips in DIPs.
    /// </summary>
    public virtual float ToolTipBaseSize { get; set; } = 8;

    /// <summary>
    /// Gets or sets the vertical offset applied to tooltips relative to the cursor.
    /// </summary>
    public virtual float ToolTipVerticalOffset { get; set; } = 20; // don't know any API where to get that info from (that would support system cursor size change by end-user)

    /// <summary>
    /// Gets or sets the tooltip shadow blur radius.
    /// </summary>
    public virtual float ToolTipShadowBlurRadius { get; set; } = 4;

    /// <summary>
    /// Gets or sets the tooltip corner radius.
    /// </summary>
    public virtual float ToolTipCornerRadius { get; set; } = 3;

    /// <summary>
    /// Gets or sets the tooltip margin.
    /// </summary>
    public virtual float ToolTipMargin { get; set; } = 4;

    /// <summary>
    /// Gets or sets the initial delay for showing tooltips, derived from the system double-click time.
    /// </summary>
    public virtual uint ToolTipInitialTime { get; set; } = WiceCommons.GetDoubleClickTime(); // https://docs.microsoft.com/en-us/windows/win32/controls/ttm-setdelaytime#remarks

    /// <summary>
    /// Gets or sets the total time a tooltip remains visible.
    /// </summary>
    public virtual uint ToolTipVisibleTime { get; set; }

    /// <summary>
    /// Gets or sets the delay before showing a tooltip again when moving between targets.
    /// </summary>
    public virtual uint ToolTipReshowTime { get; set; }

    /// <summary>
    /// Gets or sets the color of dialog drop shadows.
    /// </summary>
    public virtual D3DCOLORVALUE DialogShadowColor { get; set; } = D3DCOLORVALUE.Gray;

    /// <summary>
    /// Gets or sets the overlay color used behind modal dialog windows.
    /// </summary>
    public virtual D3DCOLORVALUE DialogWindowOverlayColor { get; set; } = D3DCOLORVALUE.Black;

    /// <summary>
    /// Gets or sets the open animation duration for dialogs.
    /// </summary>
    public virtual TimeSpan DialogOpenAnimationDuration { get; set; } = TimeSpan.FromSeconds(0.2f);

    /// <summary>
    /// Gets or sets the close animation duration for dialogs.
    /// </summary>
    public virtual TimeSpan DialogCloseAnimationDuration { get; set; } = TimeSpan.FromSeconds(0.2f);

    /// <summary>
    /// Gets or sets the blur radius of dialog shadows.
    /// </summary>
    public virtual float DialogShadowBlurRadius { get; set; } = 30;

    /// <summary>
    /// Gets or sets the opacity for the window overlay behind dialogs (0..1).
    /// </summary>
    public virtual float DialogWindowOverlayOpacity { get; set; } = 0.2f;

    /// <summary>
    /// Gets or sets the width of the header selection indicator line.
    /// </summary>
    public virtual float HeaderSelectionWidth { get; set; } = 4;

    /// <summary>
    /// Gets or sets the margin around header panels.
    /// </summary>
    public virtual D2D_RECT_F HeaderPanelMargin { get; set; } = D2D_RECT_F.Thickness(10);

    /// <summary>
    /// Gets or sets the margin around the header close button.
    /// </summary>
    public virtual D2D_RECT_F HeaderCloseButtonMargin { get; set; } = D2D_RECT_F.Thickness(20, 0, 0, 0);

    /// <summary>
    /// Gets or sets the title bar font size in points.
    /// </summary>
    public virtual float TitleBarFontSize { get; set; } = 12f;

    /// <summary>
    /// Gets or sets the title bar content margin.
    /// </summary>
    public virtual D2D_RECT_F TitleBarMargin { get; set; } = D2D_RECT_F.Thickness(10, 0, 0, 0);

    /// <summary>
    /// Gets the vertical scrollbar width in device-independent pixels for the specified DPI.
    /// </summary>
    /// <param name="dpi">Target DPI (96 = 100%).</param>
    public virtual int GetVerticalScrollBarWidth(uint dpi) => WiceCommons.GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX.SM_CXVSCROLL, dpi);

    /// <summary>
    /// Gets the horizontal scrollbar height in device-independent pixels for the specified DPI.
    /// </summary>
    /// <param name="dpi">Target DPI (96 = 100%).</param>
    public virtual int GetHorizontalScrollBarHeight(uint dpi) => WiceCommons.GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX.SM_CXHSCROLL, dpi);

    /// <summary>
    /// Gets the overlay scrollbar thickness in device-independent pixels for the specified DPI.
    /// </summary>
    /// <param name="dpi">Target DPI (96 = 100%).</param>
    public virtual int GetScrollBarOverlaySize(uint dpi) => WiceCommons.GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX.SM_CXVSCROLL, dpi) / 3;

    /// <summary>
    /// Initializes the theme using the given native window, applying initial DPI scaling if necessary.
    /// Called only once.
    /// </summary>
    /// <param name="native">The native window wrapper.</param>
    /// <returns>
    /// True if the DPI changed and scaling was applied; otherwise false (including when DPI awareness is UNWARE).
    /// </returns>
    protected virtual internal bool Initialize(NativeWindow native)
    {
        ExceptionExtensions.ThrowIfNull(native, nameof(native));
        if (_window == null)
            throw new InvalidOperationException();

        var dpiAwareness = native.DpiAwareness;
        if (WiceCommons.AreDpiAwarenessContextsEqual(dpiAwareness, DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_UNAWARE))
            return false;

        var newDpi = native.Dpi;
        if (newDpi != CurrentDpi)
        {
            UpdateDpi(CurrentDpi, newDpi);
            var e = new ThemeDpiEventArgs(CurrentDpi, newDpi);
            CurrentDpi = newDpi;
            OnDpiChanged(this, e);
            _window.OnThemeDpiEvent(this, e);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Updates the theme for a potential DPI change using the bound <see cref="Window"/>.
    /// </summary>
    /// <returns>True when DPI changed and an update was applied; otherwise false.</returns>
    protected virtual internal bool Update()
    {
        if (_window == null)
            throw new InvalidOperationException();

        var dpiAwareness = _window.DpiAwareness;
        if (WiceCommons.AreDpiAwarenessContextsEqual(dpiAwareness, DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_UNAWARE))
            return false;

        var newDpi = _window.Dpi;
        if (newDpi != CurrentDpi)
        {
            UpdateDpi(CurrentDpi, newDpi);
            var e = new ThemeDpiEventArgs(CurrentDpi, newDpi);
            CurrentDpi = newDpi;
            OnDpiChanged(this, e);
            _window.OnThemeDpiEvent(this, e);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Invokes <see cref="DpiChanged"/> with the provided arguments.
    /// </summary>
    /// <param name="sender">The sender of the DPI event.</param>
    /// <param name="e">Event data describing the DPI change.</param>
    protected virtual void OnDpiChanged(object sender, ThemeDpiEventArgs e) => DpiChanged?.Invoke(sender, e);

    /// <summary>
    /// Applies DPI scaling to theme metrics and sizes when the DPI changes.
    /// </summary>
    /// <param name="oldDpi">The previous DPI value.</param>
    /// <param name="newDpi">The new DPI value.</param>
    protected virtual void UpdateDpi(uint oldDpi, uint newDpi)
    {
        DefaultDefaultFontSize = UIExtensions.DpiScale(DefaultDefaultFontSize, oldDpi, newDpi);
        DefaultDefaultSplitterSize = UIExtensions.DpiScale(DefaultDefaultSplitterSize, oldDpi, newDpi);
        if (_defaultFontSize > 0)
        {
            DefaultFontSize = UIExtensions.DpiScale(DefaultFontSize, oldDpi, newDpi);
        }

        if (_defaultRichTextFontSize > 0)
        {
            DefaultRichTextFontSize = UIExtensions.DpiScale(DefaultRichTextFontSize, oldDpi, newDpi);
        }

        if (_defaultSplitterSize > 0)
        {
            DefaultSplitterSize = UIExtensions.DpiScale(DefaultSplitterSize, oldDpi, newDpi);
        }

        ScrollBarArrowMargin = UIExtensions.DpiScale(ScrollBarArrowMargin, oldDpi, newDpi);
        ToolTipBaseSize = UIExtensions.DpiScale(ToolTipBaseSize, oldDpi, newDpi);
        ToolTipVerticalOffset = UIExtensions.DpiScale(ToolTipVerticalOffset, oldDpi, newDpi);
        ToolTipShadowBlurRadius = UIExtensions.DpiScale(ToolTipShadowBlurRadius, oldDpi, newDpi);
        ToolTipCornerRadius = UIExtensions.DpiScale(ToolTipCornerRadius, oldDpi, newDpi);
        ToolTipMargin = UIExtensions.DpiScale(ToolTipMargin, oldDpi, newDpi);
        DialogShadowBlurRadius = UIExtensions.DpiScale(DialogShadowBlurRadius, oldDpi, newDpi);
        ScrollBarOverlayCornerRadius = UIExtensions.DpiScale(ScrollBarOverlayCornerRadius, oldDpi, newDpi);
        ScrollBarButtonStrokeThickness = UIExtensions.DpiScale(ScrollBarButtonStrokeThickness, oldDpi, newDpi);
        HeaderSelectionWidth = UIExtensions.DpiScale(HeaderSelectionWidth, oldDpi, newDpi);
        FocusThickness = UIExtensions.DpiScale(FocusThickness, oldDpi, newDpi);
        FocusOffset = UIExtensions.DpiScale(FocusOffset, oldDpi, newDpi);
        BoxSize = UIExtensions.DpiScale(BoxSize, oldDpi, newDpi);
        BorderSize = UIExtensions.DpiScale(BorderSize, oldDpi, newDpi);
        ToggleBorderRatio = UIExtensions.DpiScale(ToggleBorderRatio, oldDpi, newDpi);
        ToggleRadiusRatio = UIExtensions.DpiScale(ToggleRadiusRatio, oldDpi, newDpi);
        ButtonMargin = UIExtensions.DpiScale(ButtonMargin, oldDpi, newDpi);
        ButtonMinWidth = UIExtensions.DpiScale(ButtonMinWidth, oldDpi, newDpi);
        DialogBoxButtonFontSize = UIExtensions.DpiScale(DialogBoxButtonFontSize, oldDpi, newDpi);
        RoundedButtonCornerRadius = UIExtensions.DpiScale(RoundedButtonCornerRadius, oldDpi, newDpi);
        HeaderSelectionWidth = UIExtensions.DpiScale(HeaderSelectionWidth, oldDpi, newDpi);
        HeaderCloseButtonMargin = UIExtensions.DpiScaleThickness(HeaderCloseButtonMargin, oldDpi, newDpi);
        HeaderPanelMargin = UIExtensions.DpiScaleThickness(HeaderPanelMargin, oldDpi, newDpi);
        TitleBarFontSize = UIExtensions.DpiScale(TitleBarFontSize, oldDpi, newDpi);
        TitleBarMargin = UIExtensions.DpiScaleThickness(TitleBarMargin, oldDpi, newDpi);
        MessageBoxPadding = UIExtensions.DpiScale(MessageBoxPadding, oldDpi, newDpi);
        StateButtonListPadding = UIExtensions.DpiScale(StateButtonListPadding, oldDpi, newDpi);
    }
}