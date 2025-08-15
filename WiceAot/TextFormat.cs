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

    /// <summary>
    /// Builds a deterministic string key that represents the given DirectWrite font collection.
    /// </summary>
    /// <param name="fonts">The font collection to serialize; <see langword="null"/> yields <see langword="null"/>.</param>
    /// <returns>
    /// A stable string suitable for use as part of a cache key, or <see langword="null"/> when the
    /// collection is <see langword="null"/> or contains no families.
    /// </returns>
    /// <remarks>
    /// - The key encodes the number of families and each family's localized names as
    ///   "localeName␀string" pairs separated by U+0000 (NUL) to minimize accidental collisions.
    /// - The order provided by the collection is preserved to maintain determinism.
    /// </remarks>
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

    /// <summary>
    /// Builds a composite deterministic cache key for the effective text-formatting values.
    /// </summary>
    /// <param name="text">The source <see cref="ITextFormat"/> whose values are encoded.</param>
    /// <param name="family">The effective font family name to encode (may be <see langword="null"/>).</param>
    /// <param name="size">The effective font size in DIPs to encode.</param>
    /// <returns>
    /// A stable string key that combines the family, size, font collection identity, and all relevant
    /// DirectWrite formatting attributes.
    /// </returns>
    /// <remarks>
    /// - Uses U+0000 (NUL) as a field separator to reduce ambiguity and collisions.
    /// - Encodes, in order: family, size, font collection key, weight, style, stretch, trimming granularity,
    ///   word wrapping, paragraph alignment, text alignment, flow direction, and reading direction.
    /// - Intended to be used as a dictionary key for caching format/layout resources.
    /// </remarks>
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
