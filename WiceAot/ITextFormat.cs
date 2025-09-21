namespace Wice;

/// <summary>
/// Defines a set of text formatting options used by implementations to create or configure
/// text formatting/layout (e.g., DirectWrite text formats).
/// </summary>
public interface ITextFormat
{
    /// <summary>
    /// Gets or sets the font family name (e.g., "Segoe UI").
    /// </summary>
    string? FontFamilyName { get; set; }

    /// <summary>
    /// Gets or sets the font collection used to resolve the <see cref="FontFamilyName"/>.
    /// </summary>
    IComObject<IDWriteFontCollection>? FontCollection { get; set; }

    /// <summary>
    /// Gets or sets the font size in device-independent pixels (DIPs).
    /// </summary>
    float? FontSize { get; set; }

    /// <summary>
    /// Gets or sets the font weight.
    /// </summary>
    DWRITE_FONT_WEIGHT FontWeight { get; set; }

    /// <summary>
    /// Gets or sets the font style (e.g., Normal, Italic, Oblique).
    /// </summary>
    DWRITE_FONT_STYLE FontStyle { get; set; }

    /// <summary>
    /// Gets or sets the font stretch (width) from ultra-condensed to ultra-expanded.
    /// </summary>
    DWRITE_FONT_STRETCH FontStretch { get; set; }

    /// <summary>
    /// Gets or sets the paragraph alignment (vertical alignment within the layout box).
    /// </summary>
    DWRITE_PARAGRAPH_ALIGNMENT ParagraphAlignment { get; set; }

    /// <summary>
    /// Gets or sets the text alignment (horizontal alignment within the layout box).
    /// </summary>
    DWRITE_TEXT_ALIGNMENT Alignment { get; set; }

    /// <summary>
    /// Gets or sets the flow direction for the layout (e.g., top-to-bottom, left-to-right).
    /// </summary>
    DWRITE_FLOW_DIRECTION FlowDirection { get; set; }

    /// <summary>
    /// Gets or sets the reading direction (e.g., left-to-right, right-to-left).
    /// </summary>
    DWRITE_READING_DIRECTION ReadingDirection { get; set; }

    /// <summary>
    /// Gets or sets the word wrapping behavior.
    /// </summary>
    DWRITE_WORD_WRAPPING WordWrapping { get; set; }

    /// <summary>
    /// Gets or sets the trimming granularity used when text is truncated.
    /// </summary>
    DWRITE_TRIMMING_GRANULARITY TrimmingGranularity { get; set; }
}
