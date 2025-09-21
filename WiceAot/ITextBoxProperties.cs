namespace Wice;

/// <summary>
/// Defines configurable text rendering settings for a text box, including antialiasing mode,
/// draw options, and DirectWrite rendering parameters.
/// </summary>
public interface ITextBoxProperties
{
    /// <summary>
    /// Gets or sets the text antialiasing mode used when rasterizing glyphs.
    /// </summary>
    D2D1_TEXT_ANTIALIAS_MODE AntiAliasingMode { get; set; }

    /// <summary>
    /// Gets or sets the draw options that control how text is rendered and measured.
    /// </summary>
    D2D1_DRAW_TEXT_OPTIONS DrawOptions { get; set; }

    /// <summary>
    /// Gets or sets the DirectWrite rendering parameters applied when drawing text.
    /// </summary>
    TextRenderingParameters TextRenderingParameters { get; set; }
}
