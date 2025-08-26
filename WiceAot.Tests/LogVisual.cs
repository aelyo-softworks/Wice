using System.Text;
using Windows.UI.Composition;

namespace WiceAot.Tests;

// this is a Direct2D/DirectWrite visual, so we derive from RenderVisual
public class LogVisual : RenderVisual, IDisposable
{
    // we just expose some properties for font family and size, color (RenderVisual already has BackgroundColor)
    public static VisualProperty FontFamilyNameProperty { get; } = VisualProperty.Add(typeof(LogVisual), nameof(FontFamilyName), VisualPropertyInvalidateModes.Measure, "Cascadia Mono");
    public static VisualProperty FontSizeProperty { get; } = VisualProperty.Add<float?>(typeof(LogVisual), nameof(FontSize), VisualPropertyInvalidateModes.Measure);
    public static VisualProperty ForegroundColorProperty { get; } = VisualProperty.Add<D3DCOLORVALUE>(typeof(LogVisual), nameof(ForegroundColor), VisualPropertyInvalidateModes.Render);

    // this is where we store our log lines
    // that could be some circular rolling buffer instead
    private readonly List<string> _lines = [];

    // cached information
    private IComObject<ID2D1Brush>? _textColor;
    private IComObject<IDWriteTextLayout>? _layout;
    private DWRITE_TEXT_METRICS1 _metrics;
    private int _firstViewableEntryIndex = -1;
    private int _lastViewableEntryIndex = -1;
    private float _lineHeight;
    private float _largestWidth;
    private bool _disposedValue;

    // public properties
    public string? FontFamilyName { get => (string?)GetPropertyValue(FontFamilyNameProperty)!; set => SetPropertyValue(FontFamilyNameProperty, value); }
    public float? FontSize { get => (float?)GetPropertyValue(FontSizeProperty); set => SetPropertyValue(FontSizeProperty, value); }
    public D3DCOLORVALUE ForegroundColor { get => (D3DCOLORVALUE)GetPropertyValue(ForegroundColorProperty)!; set => SetPropertyValue(ForegroundColorProperty, value); }

    // append a line to the log
    public virtual void Append(string text)
    {
        _lines.Add(text);
        Invalidate(VisualPropertyInvalidateModes.Measure);
    }

    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _layout?.Dispose();
                _textColor?.Dispose();
            }

            _disposedValue = true;
        }
    }

    // this is called by our parent to measure us
    // this is where we give the size/extent we need to show all lines (which we'll never really do)
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        ComputeLayout();
        return new D2D_SIZE_F(_largestWidth, _lines.Count * _lineHeight);
    }

    // we're virtualized so the composition visual size is the size of lines we draw, not more
    // we don't call base.SetCompositionVisualSizeAndOffset on purpose
    protected override void SetCompositionVisualSizeAndOffset(ContainerVisual visual)
    {
        ComputeLayout();
        var rr = RelativeRenderRect;
        visual.Size = new Vector2(Math.Max(_metrics.Base.width, rr.Width), Math.Min(_metrics.Base.height, rr.Height));
        visual.Offset = RenderOffset;
    }

    // we're called by Wice to render on the composition visual (using Direct2D)
    protected override void RenderCore(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        base.RenderCore(context); // this handles background (clear, etc.)
        var layout = ComputeLayout();
        if (layout == null)
            return;

        // just render the lines we have precomputed
        _textColor ??= context.CreateSolidColorBrush(ForegroundColor);
        context.DeviceContext.DrawTextLayout(new D2D_POINT_2F(), layout, _textColor, D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT);
    }

    // text format definition is determined here
    protected virtual IComObject<IDWriteTextFormat> GetFormat()
    {
        var theme = GetWindowTheme();
        var fontSizeValue = Application.CurrentResourceManager.GetFontSize(theme, FontSize);
        return Application.CurrentResourceManager.GetTextFormat(theme,
            FontFamilyName,
            fontSizeValue,
            DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_NEAR,
            DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_LEADING
            )!;
    }

    // get parent scrollviewer if any
    protected ScrollViewer? GetScrollViewer() => Parent as ScrollViewer ?? (Parent as Viewer)?.Parent as ScrollViewer;

    // get "normal" line height for the current font
    // note this wouldn't handle complex scripts, but for log lines it's fine
    protected virtual float GetLineHeight()
    {
        if (_lineHeight == 0)
        {
            using var layout = Application.CurrentResourceManager.CreateTextLayout(GetFormat(), "The quick brown fox jumps over the lazy dog");
            var metrics = layout.GetMetrics1();
            _lineHeight = metrics.Base.height / metrics.Base.lineCount;
        }
        return _lineHeight;
    }

    // compute the text layout for the visible lines only
    protected virtual IComObject<IDWriteTextLayout>? ComputeLayout()
    {
        var lineHeight = GetLineHeight();

        // only compute if we have lines and a valid height
        if (_lines.Count == 0)
            return null;

        var height = ArrangedRect.Height;
        if (height.IsInvalid() || height <= 0)
            return null;

        // ensure we only layout the visible lines if we are in a scroll viewer
        var offset = GetScrollViewer()?.VerticalOffset ?? 0;
        var firstEntry = Math.Max(0, Math.Min(_lines.Count - 1, (offset / lineHeight).FloorI()));
        var lastEntry = Math.Max(0, Math.Min(_lines.Count - 1, ((offset + height) / lineHeight).CeilingI()));

        // cache computed layout for a bit of perf gain if the visible entries did not change
        if (_layout != null && !_layout.IsDisposed && firstEntry == _firstViewableEntryIndex && lastEntry == _lastViewableEntryIndex)
            return _layout;

        // build the text to layout, only composed of the visible lines
        var sb = new StringBuilder();
        for (var i = firstEntry; i <= lastEntry; i++)
        {
            sb.AppendLine(_lines[i]);
        }

        // dispose previous layout
        _layout?.Dispose();

        // compute Direct Write layout
        _layout = Application.CurrentResourceManager.CreateTextLayout(GetFormat(), sb.ToString());
        _metrics = _layout.GetMetrics1();

        // remember the largest width for horizontal scrolling
        if (_metrics.Base.width > _largestWidth)
        {
            _largestWidth = _metrics.Base.width;
        }

        _firstViewableEntryIndex = firstEntry;
        _lastViewableEntryIndex = lastEntry;
        return _layout;
    }
}
