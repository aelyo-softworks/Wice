using System;
using System.Drawing;
using DirectN;
using Wice.Utilities;

namespace Wice
{
    public class Theme
    {
        private string _symbolFontName;
        private string _legacySymbolFontName;
        private string _defaultFontFamilyName;
        private float _defaultFontSize;
        private float _defaultSplitterSize;

        public Theme()
        {
            ScrollBarArrowMargin = 4;

            ToolTipBaseSize = 8;
            ToolTipVerticalOffset = 20; // don't know any API where to get that info from (that would support system cursor size change by end-user)
            ToolTipColor = _D3DCOLORVALUE.FromColor(SystemColors.Info);
            ToolTipShadowBlurRadius = 4;
            ToolTipCornerRadius = 3;

            // https://docs.microsoft.com/en-us/windows/win32/controls/ttm-setdelaytime#remarks
            ToolTipInitialTime = WindowsFunctions.GetDoubleClickTime();
            ToolTipVisibleTime = ToolTipInitialTime * 10;
            ToolTipReshowTime = ToolTipInitialTime / 5;

            DialogShadowBlurRadius = 30;
            DialogShadowColor = _D3DCOLORVALUE.Gray;
            DialogOpenAnimationDuration = TimeSpan.FromSeconds(0.2f);
            DialogCloseAnimationDuration = TimeSpan.FromSeconds(0.2f);
            DialogWindowOverlayColor = _D3DCOLORVALUE.Black;
            DialogWindowOverlayOpacity = 0.2f;

            SelectionBrushAnimationDuration = TimeSpan.FromSeconds(0.3f);
            BrushAnimationDuration = TimeSpan.FromSeconds(0.3f);
            SelectedAnimationDuration = TimeSpan.FromSeconds(0.3f);

            ScrollBarBackgroundColor = new _D3DCOLORVALUE(0xFFE9E9E9);
            ScrollBarOverlayBackgroundColor = new _D3DCOLORVALUE(0x40E9E9E9);
            ScrollBarButtonStrokeColor = new _D3DCOLORVALUE(0xFF969696);
            ScrollBarButtonHoverColor = new _D3DCOLORVALUE(0xFFC9C9C9);
            ScrollBarThumbColor = new _D3DCOLORVALUE(0xFFC9C9C9);
            ScrollBarOverlayThumbColor = new _D3DCOLORVALUE(0xA0393939);
            VerticalScrollBarWidth = WindowsFunctions.GetSystemMetrics(SM.SM_CXVSCROLL);
            HorizontalScrollBarHeight = WindowsFunctions.GetSystemMetrics(SM.SM_CXHSCROLL);
            ScrollBarOverlaySize = WindowsFunctions.GetSystemMetrics(SM.SM_CXVSCROLL) / 3;
            ScrollBarOverlayCornerRadius = 5;
            ScrollBarButtonStrokeThickness = 1;

            TextBoxForegroundColor = ColorUtilities.GetSysColor(COLOR.COLOR_WINDOWTEXT);
            TextBoxSelectionColor = ColorUtilities.GetSysColor(COLOR.COLOR_HIGHLIGHTTEXT);

            SelectedColor = new _D3DCOLORVALUE(0xFF0078D7); // blue
            SelectedHoverColor = new _D3DCOLORVALUE(0xFFCBE8F6);
            HeaderSelectionWidth = 4;

            FocusColor = _D3DCOLORVALUE.Black;
            FocusThickness = 1f;
            FocusOffset = -2f;
            FocusDashArray = new float[] { 2 };

            BoxSize = 20;
            BorderSize = 4;
            ToggleBorderRatio = 0.07f;
            BorderColor = _D3DCOLORVALUE.Black;
            UnselectedColor = _D3DCOLORVALUE.White;
            SplitterColor = ColorUtilities.GetSysColor(COLOR.COLOR_ACTIVEBORDER);

            ButtonColor = new _D3DCOLORVALUE(0xFFCCCCCC);
            ButtonMargin = 6;
            ButtonMinWidth = 70; // not sure we can get this from Windows
            DialogBoxButtonFontSize = DefaultFontSize;
            RoundedButtonCornerRadius = 4;

            CaretColor = _D3DCOLORVALUE.Blue;

            ListBoxItemColor = _D3DCOLORVALUE.White;
            ListBoxHoverColor = SelectedColor;

            DisabledOpacityRatio = 0.5f;
        }

        public virtual string LegacySymbolFontName { get => _legacySymbolFontName.Nullify() ?? "Segoe UI Symbol"; set => _legacySymbolFontName = value; }
        public virtual string SymbolFontName { get => _symbolFontName.Nullify() ?? "Segoe MDL2 Assets"; set => _symbolFontName = value; }
        public virtual string DefaultFontFamilyName { get => _defaultFontFamilyName.Nullify() ?? "Segoe UI"; set => _defaultFontFamilyName = value; }
        public virtual float DefaultFontSize { get => _defaultFontSize <= 0 ? 14f : _defaultFontSize; set => _defaultFontSize = value; }
        public virtual float DefaultSplitterSize { get => _defaultSplitterSize <= 0 ? 5f : _defaultSplitterSize; set => _defaultSplitterSize = value; }

        public virtual _D3DCOLORVALUE LinkColor { get; set; }
        public virtual _D3DCOLORVALUE HoverLinkColor { get; set; }
        public virtual _D3DCOLORVALUE SplitterColor { get; set; }
        public virtual _D3DCOLORVALUE BorderColor { get; set; }
        public virtual _D3DCOLORVALUE UnselectedColor { get; set; }
        public virtual _D3DCOLORVALUE SelectedColor { get; set; }
        public virtual float BorderSize { get; set; }
        public virtual float BoxSize { get; set; }
        public virtual float ToggleBorderRatio { get; set; }
        public virtual float DisabledOpacityRatio { get; set; }

        public virtual _D3DCOLORVALUE ListBoxItemColor { get; set; }
        public virtual _D3DCOLORVALUE ListBoxHoverColor { get; set; }

        public virtual _D3DCOLORVALUE FocusColor { get; set; }
        public virtual float FocusOffset { get; set; }
        public virtual float FocusThickness { get; set; }
        public virtual float[] FocusDashArray { get; set; }

        public virtual _D3DCOLORVALUE ButtonColor { get; set; }
        public virtual float ButtonMargin { get; set; }
        public virtual float ButtonMinWidth { get; set; }
        public virtual float DialogBoxButtonFontSize { get; set; }
        public virtual float RoundedButtonCornerRadius { get; set; }

        public virtual _D3DCOLORVALUE CaretColor { get; set; }

        public virtual float ScrollBarButtonStrokeThickness { get; set; }
        public virtual _D3DCOLORVALUE ScrollBarBackgroundColor { get; set; }
        public virtual _D3DCOLORVALUE ScrollBarOverlayBackgroundColor { get; set; }
        public virtual _D3DCOLORVALUE ScrollBarButtonStrokeColor { get; set; }
        public virtual _D3DCOLORVALUE ScrollBarButtonHoverColor { get; set; }
        public virtual _D3DCOLORVALUE ScrollBarThumbColor { get; set; }
        public virtual _D3DCOLORVALUE ScrollBarOverlayThumbColor { get; set; }
        public virtual int ScrollBarArrowMargin { get; set; }
        public virtual int ScrollBarOverlaySize { get; set; }
        public virtual int ScrollBarOverlayCornerRadius { get; set; }
        public virtual int VerticalScrollBarWidth { get; set; }
        public virtual int HorizontalScrollBarHeight { get; set; }

        public virtual _D3DCOLORVALUE TextBoxForegroundColor { get; set; }
        public virtual _D3DCOLORVALUE TextBoxSelectionColor { get; set; }

        public virtual TimeSpan SelectionBrushAnimationDuration { get; set; }
        public virtual _D3DCOLORVALUE SelectedHoverColor { get; set; }

        public virtual TimeSpan BrushAnimationDuration { get; set; }
        public virtual TimeSpan SelectedAnimationDuration { get; set; }

        public virtual _D3DCOLORVALUE ToolTipColor { get; set; }
        public virtual float ToolTipBaseSize { get; set; }
        public virtual float ToolTipVerticalOffset { get; set; }
        public virtual float ToolTipShadowBlurRadius { get; set; }
        public virtual float ToolTipCornerRadius { get; set; }
        public virtual int ToolTipInitialTime { get; set; }
        public virtual int ToolTipVisibleTime { get; set; }
        public virtual int ToolTipReshowTime { get; set; }

        public virtual _D3DCOLORVALUE DialogShadowColor { get; set; }
        public virtual _D3DCOLORVALUE DialogWindowOverlayColor { get; set; }
        public virtual TimeSpan DialogOpenAnimationDuration { get; set; }
        public virtual TimeSpan DialogCloseAnimationDuration { get; set; }
        public virtual float DialogShadowBlurRadius { get; set; }
        public virtual float DialogWindowOverlayOpacity { get; set; }

        public virtual float HeaderSelectionWidth { get; set; }
    }
}