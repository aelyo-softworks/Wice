namespace Wice;

/// <summary>
/// Represents a mutable set of text formatting options (backed by DirectWrite concepts).
/// </summary>
/// <remarks>
/// Implements <see cref="ITextFormat"/> and provides helpers to build deterministic cache keys
/// for sharing/reusing text-format resources across renderers/layouts.
/// </remarks>
public class TextFormat : ITextFormat
{
    /// <summary>
    /// Initializes a new instance of <see cref="TextFormat"/> with DirectWrite defaults:
    /// <see cref="DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_NORMAL"/>,
    /// <see cref="DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_NORMAL"/>,
    /// <see cref="DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_NORMAL"/>.
    /// </summary>
    public TextFormat()
    {
        FontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_NORMAL;
        FontStyle = DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_NORMAL;
        FontStretch = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_NORMAL;
    }

    /// <inheritdoc/>
    public virtual string? FontFamilyName { get; set; }
    /// <inheritdoc/>
    public virtual IComObject<IDWriteFontCollection>? FontCollection { get; set; }
    /// <inheritdoc/>
    public virtual float? FontSize { get; set; }
    /// <inheritdoc/>
    public virtual DWRITE_FONT_WEIGHT FontWeight { get; set; }
    /// <inheritdoc/>
    public virtual DWRITE_FONT_STYLE FontStyle { get; set; }
    /// <inheritdoc/>
    public virtual DWRITE_FONT_STRETCH FontStretch { get; set; }
    /// <inheritdoc/>
    public virtual DWRITE_PARAGRAPH_ALIGNMENT ParagraphAlignment { get; set; }
    /// <inheritdoc/>
    public virtual DWRITE_TEXT_ALIGNMENT Alignment { get; set; }
    /// <inheritdoc/>
    public virtual DWRITE_FLOW_DIRECTION FlowDirection { get; set; }
    /// <inheritdoc/>
    public virtual DWRITE_READING_DIRECTION ReadingDirection { get; set; }
    /// <inheritdoc/>
    public virtual DWRITE_WORD_WRAPPING WordWrapping { get; set; }
    /// <inheritdoc/>
    public virtual DWRITE_TRIMMING_GRANULARITY TrimmingGranularity { get; set; }

    internal static string? GetCacheKey(IDWriteFontCollection? fonts)
    {
        if (fonts == null)
            return null;

        var families = fonts.GetFamilies();
        if (families.Count == 0)
            return null;

        var str = families.Count.ToString() + "\0";
        foreach (var family in fonts.GetFamilies())
        {
            var names = family.GetNames();
            if (names.Count == 0)
                continue;

            str += string.Join("\0", names.Select(n => n.LocaleName + "\0" + n.String));
        }
        return str;
    }

    internal static string GetCacheKey(ITextFormat text, string? family, float size) => family + "\0" + size + "\0" +
            GetCacheKey(text.FontCollection?.Object) + "\0" +
            (int)text.FontWeight + "\0" +
            (int)text.FontStyle + "\0" +
            (int)text.FontStretch + "\0" +
            (int)text.TrimmingGranularity + "\0" +
            (int)text.WordWrapping + "\0" +
            (int)text.ParagraphAlignment + "\0" +
            (int)text.Alignment + "\0" +
            (int)text.FlowDirection + "\0" +
            (int)text.ReadingDirection;
}
