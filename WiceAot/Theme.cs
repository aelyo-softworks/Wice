namespace Wice;

public class Theme
{
    public const string DefaultSymbolFontName = "Segoe MDL2 Assets";
    public const string DefaultLegacySymbolFontName = "Segoe UI Symbol";
    public const string DefaultDefaultFontFamilyName = "Segoe UI";

    private static Theme _default = new();
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
    private float _defaultSplitterSize;
    private readonly Window? _window;

    public event EventHandler<ThemeDpiChangedEventArgs>? DpiChanged;

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

    public float DefaultDefaultFontSize { get; protected set; } = 14f;
    public float DefaultDefaultSplitterSize { get; protected set; } = 5f;

    public uint CurrentDpi { get; private set; } = WiceCommons.USER_DEFAULT_SCREEN_DPI;
    public virtual string? LegacySymbolFontName { get => _legacySymbolFontName.Nullify() ?? DefaultLegacySymbolFontName; set => _legacySymbolFontName = value; }
    public virtual string? SymbolFontName { get => _symbolFontName.Nullify() ?? DefaultSymbolFontName; set => _symbolFontName = value; }
    public virtual string? DefaultFontFamilyName { get => _defaultFontFamilyName.Nullify() ?? DefaultDefaultFontFamilyName; set => _defaultFontFamilyName = value; }
    public virtual float DefaultFontSize { get => _defaultFontSize <= 0 ? DefaultDefaultFontSize : _defaultFontSize; set => _defaultFontSize = value; }
    public virtual float DefaultSplitterSize { get => _defaultSplitterSize <= 0 ? DefaultDefaultSplitterSize : _defaultSplitterSize; set => _defaultSplitterSize = value; }

    public virtual D3DCOLORVALUE LinkColor { get; set; }
    public virtual D3DCOLORVALUE HoverLinkColor { get; set; }
    public virtual D3DCOLORVALUE SplitterColor { get; set; } = ColorUtilities.GetSysColor(SYS_COLOR_INDEX.COLOR_ACTIVEBORDER);
    public virtual D3DCOLORVALUE BorderColor { get; set; } = D3DCOLORVALUE.Black;
    public virtual D3DCOLORVALUE UnselectedColor { get; set; } = D3DCOLORVALUE.White;
    public virtual D3DCOLORVALUE SelectedColor { get; set; } = new D3DCOLORVALUE(0xFF0078D7); // blue
    public virtual float BorderSize { get; set; } = 4;
    public virtual float BoxSize { get; set; } = 20;
    public virtual float ToggleBorderRatio { get; set; } = 0.07f;
    public virtual float DisabledOpacityRatio { get; set; } = 0.5f;

    public virtual D3DCOLORVALUE ListBoxItemColor { get; set; } = D3DCOLORVALUE.White;
    public virtual D3DCOLORVALUE ListBoxHoverColor { get; set; }

    public virtual D3DCOLORVALUE FocusColor { get; set; } = D3DCOLORVALUE.Black;
    public virtual float FocusOffset { get; set; } = -2f;
    public virtual float FocusThickness { get; set; } = 1f;
    public virtual float[] FocusDashArray { get; set; } = [2];

    public virtual D3DCOLORVALUE ButtonColor { get; set; } = new D3DCOLORVALUE(0xFFCCCCCC);
    public virtual float ButtonMargin { get; set; } = 6;
    public virtual float ButtonMinWidth { get; set; } = 70;
    public virtual float DialogBoxButtonFontSize { get; set; }
    public virtual float RoundedButtonCornerRadius { get; set; } = 4;

    public virtual D3DCOLORVALUE CaretColor { get; set; } = D3DCOLORVALUE.Blue;

    public virtual float ScrollBarButtonStrokeThickness { get; set; } = 1;
    public virtual D3DCOLORVALUE ScrollBarBackgroundColor { get; set; } = new D3DCOLORVALUE(0xFFE9E9E9);
    public virtual D3DCOLORVALUE ScrollBarOverlayBackgroundColor { get; set; } = new D3DCOLORVALUE(0x40E9E9E9);
    public virtual D3DCOLORVALUE ScrollBarButtonStrokeColor { get; set; } = new D3DCOLORVALUE(0xFF969696);
    public virtual D3DCOLORVALUE ScrollBarButtonHoverColor { get; set; } = new D3DCOLORVALUE(0xFFC9C9C9);
    public virtual D3DCOLORVALUE ScrollBarThumbColor { get; set; } = new D3DCOLORVALUE(0xFFC9C9C9);
    public virtual D3DCOLORVALUE ScrollBarOverlayThumbColor { get; set; } = new D3DCOLORVALUE(0xA0393939);
    public virtual int ScrollBarArrowMargin { get; set; } = 4;
    public virtual int ScrollBarOverlayCornerRadius { get; set; } = 5;

    public virtual D3DCOLORVALUE TextBoxForegroundColor { get; set; } = ColorUtilities.GetSysColor(SYS_COLOR_INDEX.COLOR_WINDOWTEXT);
    public virtual D3DCOLORVALUE TextBoxSelectionColor { get; set; } = ColorUtilities.GetSysColor(SYS_COLOR_INDEX.COLOR_HIGHLIGHTTEXT);

    public virtual TimeSpan SelectionBrushAnimationDuration { get; set; } = TimeSpan.FromSeconds(0.3f);
    public virtual TimeSpan BrushAnimationDuration { get; set; } = TimeSpan.FromSeconds(0.3f);
    public virtual TimeSpan SelectedAnimationDuration { get; set; } = TimeSpan.FromSeconds(0.3f);

    public virtual D3DCOLORVALUE ToolTipColor { get; set; } = new D3DCOLORVALUE(0xFFFFFFE1);
    public virtual float ToolTipBaseSize { get; set; } = 8;
    public virtual float ToolTipVerticalOffset { get; set; } = 20; // don't know any API where to get that info from (that would support system cursor size change by end-user)
    public virtual float ToolTipShadowBlurRadius { get; set; } = 4;
    public virtual float ToolTipCornerRadius { get; set; } = 3;
    public virtual uint ToolTipInitialTime { get; set; } = WiceCommons.GetDoubleClickTime(); // https://docs.microsoft.com/en-us/windows/win32/controls/ttm-setdelaytime#remarks
    public virtual uint ToolTipVisibleTime { get; set; }
    public virtual uint ToolTipReshowTime { get; set; }

    public virtual D3DCOLORVALUE DialogShadowColor { get; set; } = D3DCOLORVALUE.Gray;
    public virtual D3DCOLORVALUE DialogWindowOverlayColor { get; set; } = D3DCOLORVALUE.Black;
    public virtual TimeSpan DialogOpenAnimationDuration { get; set; } = TimeSpan.FromSeconds(0.2f);
    public virtual TimeSpan DialogCloseAnimationDuration { get; set; } = TimeSpan.FromSeconds(0.2f);
    public virtual float DialogShadowBlurRadius { get; set; } = 30;
    public virtual float DialogWindowOverlayOpacity { get; set; } = 0.2f;

    public virtual float HeaderSelectionWidth { get; set; } = 4;

    public virtual float TitleBarFontSize { get; set; } = 12f;
    public virtual D2D_RECT_F TitleBarMargin { get; set; } = D2D_RECT_F.Thickness(10, 0, 0, 0);

    public virtual int GetVerticalScrollBarWidth(uint dpi) => WiceCommons.GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX.SM_CXVSCROLL, dpi);
    public virtual int GetHorizontalScrollBarHeight(uint dpi) => WiceCommons.GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX.SM_CXHSCROLL, dpi);
    public virtual int GetScrollBarOverlaySize(uint dpi) => WiceCommons.GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX.SM_CXVSCROLL, dpi) / 3;

    // called only once
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
            CurrentDpi = newDpi;
            var e = new ThemeDpiChangedEventArgs(CurrentDpi, newDpi);
            OnDpiChanged(this, e);
            _window.OnThemeDpiChanged(this, e);
            return true;
        }
        return false;
    }

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
            CurrentDpi = newDpi;
            var e = new ThemeDpiChangedEventArgs(CurrentDpi, newDpi);
            OnDpiChanged(this, e);
            _window.OnThemeDpiChanged(this, e);
            return true;
        }
        return false;
    }

    protected virtual void OnDpiChanged(object sender, ThemeDpiChangedEventArgs e) => DpiChanged?.Invoke(sender, e);

    protected virtual void UpdateDpi(uint oldDpi, uint newDpi)
    {
        DefaultDefaultFontSize = UIExtensions.DpiScale(DefaultDefaultFontSize, oldDpi, newDpi);
        DefaultDefaultSplitterSize = UIExtensions.DpiScale(DefaultDefaultSplitterSize, oldDpi, newDpi);
        if (_defaultFontSize > 0)
        {
            DefaultFontSize = UIExtensions.DpiScale(DefaultFontSize, oldDpi, newDpi);
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
        DialogShadowBlurRadius = UIExtensions.DpiScale(DialogShadowBlurRadius, oldDpi, newDpi);
        ScrollBarOverlayCornerRadius = UIExtensions.DpiScale(ScrollBarOverlayCornerRadius, oldDpi, newDpi);
        ScrollBarButtonStrokeThickness = UIExtensions.DpiScale(ScrollBarButtonStrokeThickness, oldDpi, newDpi);
        HeaderSelectionWidth = UIExtensions.DpiScale(HeaderSelectionWidth, oldDpi, newDpi);
        FocusThickness = UIExtensions.DpiScale(FocusThickness, oldDpi, newDpi);
        FocusOffset = UIExtensions.DpiScale(FocusOffset, oldDpi, newDpi);
        BoxSize = UIExtensions.DpiScale(BoxSize, oldDpi, newDpi);
        BorderSize = UIExtensions.DpiScale(BorderSize, oldDpi, newDpi);
        ToggleBorderRatio = UIExtensions.DpiScale(ToggleBorderRatio, oldDpi, newDpi);
        ButtonMargin = UIExtensions.DpiScale(ButtonMargin, oldDpi, newDpi);
        ButtonMinWidth = UIExtensions.DpiScale(ButtonMinWidth, oldDpi, newDpi);
        DialogBoxButtonFontSize = UIExtensions.DpiScale(DialogBoxButtonFontSize, oldDpi, newDpi);
        RoundedButtonCornerRadius = UIExtensions.DpiScale(RoundedButtonCornerRadius, oldDpi, newDpi);
        HeaderSelectionWidth = UIExtensions.DpiScale(HeaderSelectionWidth, oldDpi, newDpi);
        TitleBarFontSize = UIExtensions.DpiScale(TitleBarFontSize, oldDpi, newDpi);
        TitleBarMargin = UIExtensions.DpiScaleThickness(TitleBarMargin, oldDpi, newDpi);
    }
}