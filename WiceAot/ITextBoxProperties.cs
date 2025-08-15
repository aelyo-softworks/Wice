namespace Wice;

/**
 <summary>
 Defines configurable text rendering settings for a text box, including antialiasing mode,
 draw options, and DirectWrite rendering parameters.
 </summary>
 <remarks>
 Implementations use these values to configure Direct2D/DirectWrite when drawing text,
 typically via ID2D1DeviceContext and IDWriteRenderingParams.
 </remarks>
*/
public interface ITextBoxProperties
{
    /// <summary>
    /// Gets or sets the text antialiasing mode used when rasterizing glyphs.
    /// </summary>
    /// <remarks>
    /// Maps to ID2D1DeviceContext.TextAntialiasMode. Use
    /// <see cref="D2D1_TEXT_ANTIALIAS_MODE.CLEARTYPE"/> for subpixel rendering,
    /// <see cref="D2D1_TEXT_ANTIALIAS_MODE.GRAYSCALE"/> for grayscale antialiasing,
    /// or <see cref="D2D1_TEXT_ANTIALIAS_MODE.ALIASED"/> to disable antialiasing.
    /// </remarks>
    D2D1_TEXT_ANTIALIAS_MODE AntiAliasingMode { get; set; }

    /// <summary>
    /// Gets or sets the draw options that control how text is rendered and measured.
    /// </summary>
    /// <remarks>
    /// Passed to ID2D1DeviceContext.DrawText or DrawTextLayout to control clipping,
    /// pixel snapping, and measuring behavior.
    /// </remarks>
    D2D1_DRAW_TEXT_OPTIONS DrawOptions { get; set; }

    /// <summary>
    /// Gets or sets the DirectWrite rendering parameters applied when drawing text.
    /// </summary>
    /// <remarks>
    /// Overrides device defaults for gamma, enhanced contrast, ClearType level, pixel geometry,
    /// grid fit mode, and rendering mode. Create monitor-matched parameters with
    /// <see cref="TextRenderingParameters.FromMonitor(HMONITOR)"/>.
    /// </remarks>
    TextRenderingParameters TextRenderingParameters { get; set; }
}
