namespace Wice.Samples.Gallery;

public class GalleryTheme(GalleryWindow window) : Theme(window)
{
    public virtual float MenuBackWidth { get; set; } = 250;
    public virtual D2D_RECT_F PageHeadersMargin { get; set; } = D2D_RECT_F.Thickness(10, 0);
    public virtual float PageHeaderHeight { get; set; } = 40;
    public virtual D2D_RECT_F PageHeadersTextMargin { get; set; } = D2D_RECT_F.Thickness(10, 0, 10, 0);
    public virtual D2D_RECT_F DocumentMargin { get; set; } = D2D_RECT_F.Thickness(20, 0, 0, 0);
    public virtual float RoundedButtonShadowBlurRadius { get; set; } = 8;
    public virtual Vector3 RoundedButtonShadowOffset { get; set; } = new Vector3(0, 3, 0);

    public virtual D2D_RECT_F TitledMargin { get; set; } = D2D_RECT_F.Thickness(0, 0, 0, 20);
    public virtual float TitledFontSize { get; set; } = 26;

    public virtual D2D_RECT_F SampleButtonMargin { get; set; } = D2D_RECT_F.Thickness(5);
    public virtual float SampleButtonWidth { get; set; } = 300;
    public virtual float SampleButtonHeight { get; set; } = 150;
    public virtual D2D_RECT_F SampleButtonChildMargin { get; set; } = D2D_RECT_F.Thickness(10);
    public virtual float SampleButtonTextFontSize { get; set; } = 20;

    public virtual D2D_RECT_F SampleListVisualMargin { get; set; } = D2D_RECT_F.Thickness(0, 0, 10, 10);
    public virtual float SampleListVisualFontSize { get; set; } = 18;

    public virtual D2D_RECT_F SampleVisualCodeBoxMargin { get; set; } = D2D_RECT_F.Thickness(0, 10, 0, 0);
    public virtual D2D_RECT_F SampleVisualCodeBoxPadding { get; set; } = D2D_RECT_F.Thickness(5);
    public virtual D2D_RECT_F SampleVisualBorderPadding { get; set; } = D2D_RECT_F.Thickness(10);
    public virtual float SampleVisualBorderThickness { get; set; } = 1;
    public virtual D2D_RECT_F SampleVisualDescriptionMargin { get; set; } = D2D_RECT_F.Thickness(0, 0, 0, 10);
    public virtual float SampleVisualFontSize { get; set; } = 20;

    public virtual D2D_RECT_F AboutPagePropertyGridCellMargin { get; set; } = D2D_RECT_F.Thickness(5, 0);
    public virtual D2D_RECT_F AboutPagePropertyGridMargin { get; set; } = D2D_RECT_F.Thickness(10);
    public virtual D2D_RECT_F AboutPageMouseInPointerTextBoxPadding { get; set; } = D2D_RECT_F.Thickness(10, 0);
    public virtual D2D_RECT_F AboutPageSystemInfoButtonMargin { get; set; } = D2D_RECT_F.Thickness(0, 10);
    public virtual float AboutPageSystemInfoButtonHeight { get; set; } = 30;

    public virtual D2D_RECT_F HomePageRichTextBoxPadding { get; set; } = D2D_RECT_F.Thickness(0, 0, 20, 0);

    protected override void UpdateDpi(uint oldDpi, uint newDpi)
    {
        base.UpdateDpi(oldDpi, newDpi);
        MenuBackWidth = UIExtensions.DpiScale(MenuBackWidth, oldDpi, newDpi);
        DocumentMargin = UIExtensions.DpiScaleThickness(DocumentMargin, oldDpi, newDpi);
        PageHeadersMargin = UIExtensions.DpiScaleThickness(PageHeadersMargin, oldDpi, newDpi);
        PageHeaderHeight = UIExtensions.DpiScale(PageHeaderHeight, oldDpi, newDpi);
        PageHeadersTextMargin = UIExtensions.DpiScaleThickness(PageHeadersTextMargin, oldDpi, newDpi);
        RoundedButtonShadowBlurRadius = UIExtensions.DpiScale(RoundedButtonShadowBlurRadius, oldDpi, newDpi);
        RoundedButtonShadowOffset = UIExtensions.DpiScale(RoundedButtonShadowOffset, oldDpi, newDpi);

        TitledMargin = UIExtensions.DpiScaleThickness(TitledMargin, oldDpi, newDpi);
        TitledFontSize = UIExtensions.DpiScale(TitledFontSize, oldDpi, newDpi);

        SampleButtonMargin = UIExtensions.DpiScaleThickness(SampleButtonMargin, oldDpi, newDpi);
        SampleButtonWidth = UIExtensions.DpiScale(SampleButtonWidth, oldDpi, newDpi);
        SampleButtonHeight = UIExtensions.DpiScale(SampleButtonHeight, oldDpi, newDpi);
        SampleButtonChildMargin = UIExtensions.DpiScaleThickness(SampleButtonChildMargin, oldDpi, newDpi);
        SampleButtonTextFontSize = UIExtensions.DpiScale(SampleButtonTextFontSize, oldDpi, newDpi);

        SampleListVisualMargin = UIExtensions.DpiScaleThickness(SampleListVisualMargin, oldDpi, newDpi);
        SampleListVisualFontSize = UIExtensions.DpiScale(SampleListVisualFontSize, oldDpi, newDpi);

        SampleVisualCodeBoxMargin = UIExtensions.DpiScaleThickness(SampleVisualCodeBoxMargin, oldDpi, newDpi);
        SampleVisualCodeBoxPadding = UIExtensions.DpiScaleThickness(SampleVisualCodeBoxPadding, oldDpi, newDpi);
        SampleVisualBorderPadding = UIExtensions.DpiScaleThickness(SampleVisualBorderPadding, oldDpi, newDpi);
        SampleVisualBorderThickness = UIExtensions.DpiScale(SampleVisualBorderThickness, oldDpi, newDpi);
        SampleVisualDescriptionMargin = UIExtensions.DpiScaleThickness(SampleVisualDescriptionMargin, oldDpi, newDpi);
        SampleVisualFontSize = UIExtensions.DpiScale(SampleVisualFontSize, oldDpi, newDpi);

        AboutPagePropertyGridCellMargin = UIExtensions.DpiScaleThickness(AboutPagePropertyGridCellMargin, oldDpi, newDpi);
        AboutPagePropertyGridMargin = UIExtensions.DpiScaleThickness(AboutPagePropertyGridMargin, oldDpi, newDpi);
        AboutPageMouseInPointerTextBoxPadding = UIExtensions.DpiScaleThickness(AboutPageMouseInPointerTextBoxPadding, oldDpi, newDpi);
        AboutPageSystemInfoButtonMargin = UIExtensions.DpiScaleThickness(AboutPageSystemInfoButtonMargin, oldDpi, newDpi);
        AboutPageSystemInfoButtonHeight = UIExtensions.DpiScale(AboutPageSystemInfoButtonHeight, oldDpi, newDpi);
    }
}
