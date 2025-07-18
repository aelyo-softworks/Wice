namespace Wice;

public class Theme
{
    public const float DefaultDefaultFontSize = 14f;
    public const float DefaultDefaultSplitterSize = 5f;
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

    public Theme()
    {
        ScrollBarArrowMargin = 4;

        ToolTipBaseSize = 8;
        ToolTipVerticalOffset = 20; // don't know any API where to get that info from (that would support system cursor size change by end-user)
        ToolTipColor = new D3DCOLORVALUE(0xFFFFFFE1);
        ToolTipShadowBlurRadius = 4;
        ToolTipCornerRadius = 3;

        // https://docs.microsoft.com/en-us/windows/win32/controls/ttm-setdelaytime#remarks
        ToolTipInitialTime = WiceCommons.GetDoubleClickTime();
        ToolTipVisibleTime = ToolTipInitialTime * 10;
        ToolTipReshowTime = ToolTipInitialTime / 5;

        DialogShadowBlurRadius = 30;
        DialogShadowColor = D3DCOLORVALUE.Gray;
        DialogOpenAnimationDuration = TimeSpan.FromSeconds(0.2f);
        DialogCloseAnimationDuration = TimeSpan.FromSeconds(0.2f);
        DialogWindowOverlayColor = D3DCOLORVALUE.Black;
        DialogWindowOverlayOpacity = 0.2f;

        SelectionBrushAnimationDuration = TimeSpan.FromSeconds(0.3f);
        BrushAnimationDuration = TimeSpan.FromSeconds(0.3f);
        SelectedAnimationDuration = TimeSpan.FromSeconds(0.3f);

        ScrollBarBackgroundColor = new D3DCOLORVALUE(0xFFE9E9E9);
        ScrollBarOverlayBackgroundColor = new D3DCOLORVALUE(0x40E9E9E9);
        ScrollBarButtonStrokeColor = new D3DCOLORVALUE(0xFF969696);
        ScrollBarButtonHoverColor = new D3DCOLORVALUE(0xFFC9C9C9);
        ScrollBarThumbColor = new D3DCOLORVALUE(0xFFC9C9C9);
        ScrollBarOverlayThumbColor = new D3DCOLORVALUE(0xA0393939);
        ScrollBarOverlayCornerRadius = 5;
        ScrollBarButtonStrokeThickness = 1;

        TextBoxForegroundColor = ColorUtilities.GetSysColor(SYS_COLOR_INDEX.COLOR_WINDOWTEXT);
        TextBoxSelectionColor = ColorUtilities.GetSysColor(SYS_COLOR_INDEX.COLOR_HIGHLIGHTTEXT);

        SelectedColor = new D3DCOLORVALUE(0xFF0078D7); // blue
        HeaderSelectionWidth = 4;

        FocusColor = D3DCOLORVALUE.Black;
        FocusThickness = 1f;
        FocusOffset = -2f;
        FocusDashArray = [2];

        BoxSize = 20;
        BorderSize = 4;
        ToggleBorderRatio = 0.07f;
        BorderColor = D3DCOLORVALUE.Black;
        UnselectedColor = D3DCOLORVALUE.White;
        SplitterColor = ColorUtilities.GetSysColor(SYS_COLOR_INDEX.COLOR_ACTIVEBORDER);

        ButtonColor = new D3DCOLORVALUE(0xFFCCCCCC);
        ButtonMargin = 6;
        ButtonMinWidth = 70; // not sure we can get this from Windows
        DialogBoxButtonFontSize = DefaultFontSize;
        RoundedButtonCornerRadius = 4;

        CaretColor = D3DCOLORVALUE.Blue;

        ListBoxItemColor = D3DCOLORVALUE.White;
        ListBoxHoverColor = SelectedColor;

        DisabledOpacityRatio = 0.5f;
    }

    public virtual string? LegacySymbolFontName { get => _legacySymbolFontName.Nullify() ?? DefaultLegacySymbolFontName; set => _legacySymbolFontName = value; }
    public virtual string? SymbolFontName { get => _symbolFontName.Nullify() ?? DefaultSymbolFontName; set => _symbolFontName = value; }
    public virtual string? DefaultFontFamilyName { get => _defaultFontFamilyName.Nullify() ?? DefaultDefaultFontFamilyName; set => _defaultFontFamilyName = value; }
    public virtual float DefaultFontSize { get => _defaultFontSize <= 0 ? DefaultDefaultFontSize : _defaultFontSize; set => _defaultFontSize = value; }
    public virtual float DefaultSplitterSize { get => _defaultSplitterSize <= 0 ? DefaultDefaultSplitterSize : _defaultSplitterSize; set => _defaultSplitterSize = value; }

    public virtual D3DCOLORVALUE LinkColor { get; set; }
    public virtual D3DCOLORVALUE HoverLinkColor { get; set; }
    public virtual D3DCOLORVALUE SplitterColor { get; set; }
    public virtual D3DCOLORVALUE BorderColor { get; set; }
    public virtual D3DCOLORVALUE UnselectedColor { get; set; }
    public virtual D3DCOLORVALUE SelectedColor { get; set; }
    public virtual float BorderSize { get; set; }
    public virtual float BoxSize { get; set; }
    public virtual float ToggleBorderRatio { get; set; }
    public virtual float DisabledOpacityRatio { get; set; }

    public virtual D3DCOLORVALUE ListBoxItemColor { get; set; }
    public virtual D3DCOLORVALUE ListBoxHoverColor { get; set; }

    public virtual D3DCOLORVALUE FocusColor { get; set; }
    public virtual float FocusOffset { get; set; }
    public virtual float FocusThickness { get; set; }
    public virtual float[] FocusDashArray { get; set; }

    public virtual D3DCOLORVALUE ButtonColor { get; set; }
    public virtual float ButtonMargin { get; set; }
    public virtual float ButtonMinWidth { get; set; }
    public virtual float DialogBoxButtonFontSize { get; set; }
    public virtual float RoundedButtonCornerRadius { get; set; }

    public virtual D3DCOLORVALUE CaretColor { get; set; }

    public virtual float ScrollBarButtonStrokeThickness { get; set; }
    public virtual D3DCOLORVALUE ScrollBarBackgroundColor { get; set; }
    public virtual D3DCOLORVALUE ScrollBarOverlayBackgroundColor { get; set; }
    public virtual D3DCOLORVALUE ScrollBarButtonStrokeColor { get; set; }
    public virtual D3DCOLORVALUE ScrollBarButtonHoverColor { get; set; }
    public virtual D3DCOLORVALUE ScrollBarThumbColor { get; set; }
    public virtual D3DCOLORVALUE ScrollBarOverlayThumbColor { get; set; }
    public virtual int ScrollBarArrowMargin { get; set; }
    public virtual int ScrollBarOverlayCornerRadius { get; set; }

    public virtual D3DCOLORVALUE TextBoxForegroundColor { get; set; }
    public virtual D3DCOLORVALUE TextBoxSelectionColor { get; set; }

    public virtual TimeSpan SelectionBrushAnimationDuration { get; set; }

    public virtual TimeSpan BrushAnimationDuration { get; set; }
    public virtual TimeSpan SelectedAnimationDuration { get; set; }

    public virtual D3DCOLORVALUE ToolTipColor { get; set; }
    public virtual float ToolTipBaseSize { get; set; }
    public virtual float ToolTipVerticalOffset { get; set; }
    public virtual float ToolTipShadowBlurRadius { get; set; }
    public virtual float ToolTipCornerRadius { get; set; }
    public virtual uint ToolTipInitialTime { get; set; }
    public virtual uint ToolTipVisibleTime { get; set; }
    public virtual uint ToolTipReshowTime { get; set; }

    public virtual D3DCOLORVALUE DialogShadowColor { get; set; }
    public virtual D3DCOLORVALUE DialogWindowOverlayColor { get; set; }
    public virtual TimeSpan DialogOpenAnimationDuration { get; set; }
    public virtual TimeSpan DialogCloseAnimationDuration { get; set; }
    public virtual float DialogShadowBlurRadius { get; set; }
    public virtual float DialogWindowOverlayOpacity { get; set; }

    public virtual float HeaderSelectionWidth { get; set; }

    public virtual int GetVerticalScrollBarWidth(uint dpi) => WiceCommons.GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX.SM_CXVSCROLL, dpi);
    public virtual int GetHorizontalScrollBarHeight(uint dpi) => WiceCommons.GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX.SM_CXHSCROLL, dpi);
    public virtual int GetScrollBarOverlaySize(uint dpi) => WiceCommons.GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX.SM_CXVSCROLL, dpi) / 3;
}