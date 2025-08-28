using System.Text;
using Windows.UI.Composition;

namespace WiceAot.Tests;

// this is a Direct2D/DirectWrite visual, so we derive from RenderVisual
public class FastLogVisual : RenderVisual, IDisposable
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
    private readonly StringBuilder _sb = new();
    private bool _disposedValue;
    private float _lineHeight;

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
                _textColor?.Dispose();
            }

            _disposedValue = true;
        }
    }

    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == FontFamilyNameProperty || property == FontFamilyNameProperty)
        {
            // reset line height cache
            _lineHeight = 0;
            return true;
        }

        if (property == ForegroundColorProperty)
        {
            // reset text color cache
            var textColor = _textColor;
            _textColor = null;
            textColor?.Dispose();
            return true;
        }

        return true;
    }

    // this is called by our parent to measure us
    // here we just assume parent's width and compute height based on number of lines and line height
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint) => new(0, _lines.Count * GetLineHeight());

    // we're virtualized so the composition visual size is the size of lines we draw, not more
    // we don't call base.SetCompositionVisualSizeAndOffset on purpose
    protected override void SetCompositionVisualSizeAndOffset(ContainerVisual visual)
    {
        var ar = Parent?.ArrangedRect ?? ArrangedRect;
        visual.Size = new Vector2(ar.Width, ar.Height);
        visual.Offset = RenderOffset;
    }

    // we're called by Wice to render on the composition visual (using Direct2D)
    protected override void RenderCore(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        base.RenderCore(context); // this handles background (clear, etc.)
        var ar = Parent?.ArrangedRect ?? ArrangedRect;
        var offset = GetScrollViewer()?.VerticalOffset ?? 0;
        var lineHeight = GetLineHeight();
        var firstEntry = Math.Max(0, Math.Min(_lines.Count - 1, (offset / lineHeight).FloorI()));
        var lastEntry = Math.Max(0, Math.Min(_lines.Count - 1, ((offset + ar.Height) / lineHeight).CeilingI()));

        // build the text to layout, only composed of the visible lines
        // reuse the stringbuilder to avoid allocations
        _sb.Clear();
        for (var i = firstEntry; i <= lastEntry; i++)
        {
            _sb.AppendLine(_lines[i]);
        }

        // just render text, in this "log" case, it's faster and simpler to just draw text directly, w/o coputing a layout
        _textColor ??= context.CreateSolidColorBrush(ForegroundColor);
        context.DeviceContext.DrawText(_sb.ToString(), GetFormat(), ar, _textColor, D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT);
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
            using var layout = Application.CurrentResourceManager.CreateTextLayout(GetFormat(), string.Empty);
            var metrics = layout.GetMetrics1();
            _lineHeight = metrics.Base.height / metrics.Base.lineCount;
        }
        return _lineHeight;
    }
}
