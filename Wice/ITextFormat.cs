using DirectN;

namespace Wice
{
    public interface ITextFormat
    {
        string FontFamilyName { get; set; }
        IComObject<IDWriteFontCollection> FontCollection { get; set; }
        float? FontSize { get; set; }
        DWRITE_FONT_WEIGHT FontWeight { get; set; }
        DWRITE_FONT_STYLE FontStyle { get; set; }
        DWRITE_FONT_STRETCH FontStretch { get; set; }

        DWRITE_PARAGRAPH_ALIGNMENT ParagraphAlignment { get; set; }
        DWRITE_TEXT_ALIGNMENT Alignment { get; set; }
        DWRITE_FLOW_DIRECTION FlowDirection { get; set; }
        DWRITE_READING_DIRECTION ReadingDirection { get; set; }
        DWRITE_WORD_WRAPPING WordWrapping { get; set; }
        DWRITE_TRIMMING_GRANULARITY TrimmingGranularity { get; set; }
    }
}
