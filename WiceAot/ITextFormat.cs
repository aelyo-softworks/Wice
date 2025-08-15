namespace Wice;

/// <summary>
/// Defines a set of text formatting options used by implementations to create or configure
/// text formatting/layout (e.g., DirectWrite text formats).
/// </summary>
/// <remarks>
/// - Properties mirror common DirectWrite concepts and enums.
/// - Nullable properties allow the implementation to apply defaults or inherit values.
/// </remarks>
public interface ITextFormat
{
    /// <summary>
    /// Gets or sets the font family name (e.g., "Segoe UI").
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the implementation may apply a default or infer the family.
    /// The effective family may also be resolved against <see cref="FontCollection"/>.
    /// </remarks>
    string? FontFamilyName { get; set; }

    /// <summary>
    /// Gets or sets the font collection used to resolve the <see cref="FontFamilyName"/>.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the system or implementation default collection may be used.
    /// </remarks>
    IComObject<IDWriteFontCollection>? FontCollection { get; set; }

    /// <summary>
    /// Gets or sets the font size in device-independent pixels (DIPs).
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the implementation may provide a default size.
    /// Values should be positive finite numbers when specified.
    /// </remarks>
    float? FontSize { get; set; }

    /// <summary>
    /// Gets or sets the font weight.
    /// </summary>
    /// <seealso cref="DWRITE_FONT_WEIGHT"/>
    DWRITE_FONT_WEIGHT FontWeight { get; set; }

    /// <summary>
    /// Gets or sets the font style (e.g., Normal, Italic, Oblique).
    /// </summary>
    /// <seealso cref="DWRITE_FONT_STYLE"/>
    DWRITE_FONT_STYLE FontStyle { get; set; }

    /// <summary>
    /// Gets or sets the font stretch (width) from ultra-condensed to ultra-expanded.
    /// </summary>
    /// <seealso cref="DWRITE_FONT_STRETCH"/>
    DWRITE_FONT_STRETCH FontStretch { get; set; }

    /// <summary>
    /// Gets or sets the paragraph alignment (vertical alignment within the layout box).
    /// </summary>
    /// <seealso cref="DWRITE_PARAGRAPH_ALIGNMENT"/>
    DWRITE_PARAGRAPH_ALIGNMENT ParagraphAlignment { get; set; }

    /// <summary>
    /// Gets or sets the text alignment (horizontal alignment within the layout box).
    /// </summary>
    /// <seealso cref="DWRITE_TEXT_ALIGNMENT"/>
    DWRITE_TEXT_ALIGNMENT Alignment { get; set; }

    /// <summary>
    /// Gets or sets the flow direction for the layout (e.g., top-to-bottom, left-to-right).
    /// </summary>
    /// <seealso cref="DWRITE_FLOW_DIRECTION"/>
    DWRITE_FLOW_DIRECTION FlowDirection { get; set; }

    /// <summary>
    /// Gets or sets the reading direction (e.g., left-to-right, right-to-left).
    /// </summary>
    /// <seealso cref="DWRITE_READING_DIRECTION"/>
    DWRITE_READING_DIRECTION ReadingDirection { get; set; }

    /// <summary>
    /// Gets or sets the word wrapping behavior.
    /// </summary>
    /// <seealso cref="DWRITE_WORD_WRAPPING"/>
    DWRITE_WORD_WRAPPING WordWrapping { get; set; }

    /// <summary>
    /// Gets or sets the trimming granularity used when text is truncated.
    /// </summary>
    /// <remarks>
    /// Determines whether trimming occurs at character, word, or is disabled, etc.
    /// </remarks>
    /// <seealso cref="DWRITE_TRIMMING_GRANULARITY"/>
    DWRITE_TRIMMING_GRANULARITY TrimmingGranularity { get; set; }
}
