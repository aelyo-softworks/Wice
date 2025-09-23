namespace Wice;

/// <summary>
/// Renders an SVG image using a Direct2D device context, with optional buffering of the SVG stream to
/// avoid re-fetching during repeated renders(e.g., when size changes).
/// </summary>
public partial class SvgImage : RenderVisual, IDisposable
{
    /// <summary>
    /// Dynamic property that holds the <see cref="IReadStreamer"/> providing the SVG data.
    /// Changing this property triggers a measure pass and clears the internal buffer.
    /// </summary>
    public static VisualProperty DocumentProperty { get; } = VisualProperty.Add<IReadStreamer>(typeof(SvgImage), nameof(Document), VisualPropertyInvalidateModes.Measure);

    /// <summary>
    /// Dynamic property controlling how the SVG content is scaled to fit the available area.
    /// Changing this property triggers a measure pass.
    /// </summary>
    public static VisualProperty StretchProperty { get; } = VisualProperty.Add(typeof(SvgImage), nameof(Stretch), VisualPropertyInvalidateModes.Measure, Stretch.Uniform);

    /// <summary>
    /// Dynamic property restricting how scaling is applied (up only, down only, or both).
    /// Changing this property triggers a measure pass.
    /// </summary>
    public static VisualProperty StretchDirectionProperty { get; } = VisualProperty.Add(typeof(SvgImage), nameof(StretchDirection), VisualPropertyInvalidateModes.Measure, StretchDirection.Both);

    /// <summary>
    /// Raised after the SVG document is created for the current render pass, with the created
    /// <see cref="ID2D1SvgDocument"/> (or null when creation failed). This allows callers to inspect or
    /// modify the SVG DOM before drawing.
    /// </summary>
    public event EventHandler<ValueEventArgs<IComObject<ID2D1SvgDocument>?>>? SvgDocumentCreated;

    private bool _disposedValue;
#if NETFRAMEWORK
    private DirectN.UnmanagedMemoryStream _documentBuffer;
#else
    private DirectN.Extensions.Utilities.UnmanagedMemoryStream? _documentBuffer;
#endif
    private bool _bufferStream;

    /// <summary>
    /// Initializes a new instance of <see cref="SvgImage"/> with a transparent background and stream buffering enabled.
    /// </summary>
    public SvgImage()
    {
        BackgroundColor = D3DCOLORVALUE.Transparent;
        BufferStream = true;
    }

    /// <summary>
    /// Gets or sets whether the SVG source stream should be buffered in unmanaged memory.
    /// </summary>
    [Category(CategoryBehavior)]
    public bool BufferStream
    {
        get => _bufferStream;
        set
        {
            if (_bufferStream == value)
                return;

            _bufferStream = value;
            Interlocked.Exchange(ref _documentBuffer, null)?.Dispose();
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="IReadStreamer"/> used to obtain the SVG data.
    /// </summary>
    [Category(CategoryBehavior)]
    public IReadStreamer Document { get => (IReadStreamer)GetPropertyValue(DocumentProperty)!; set => SetPropertyValue(DocumentProperty, value); }

    /// <summary>
    /// Gets or sets how the SVG content is resized to fill the available layout slot.
    /// </summary>
    [Category(CategoryLayout)]
    public Stretch Stretch { get => (Stretch)GetPropertyValue(StretchProperty)!; set => SetPropertyValue(StretchProperty, value); }

    /// <summary>
    /// Gets or sets constraints that limit the scaling direction when applying <see cref="Stretch"/>.
    /// </summary>
    [Category(CategoryLayout)]
    public StretchDirection StretchDirection { get => (StretchDirection)GetPropertyValue(StretchDirectionProperty)!; set => SetPropertyValue(StretchDirectionProperty, value); }

    /// <summary>
    /// Invokes <see cref="SvgDocumentCreated"/> with the created SVG document.
    /// </summary>
    /// <param name="sender">The sender (typically <see cref="SvgImage"/>).</param>
    /// <param name="e">Event payload carrying the created <see cref="ID2D1SvgDocument"/> or null.</param>
    protected virtual void OnSvgDocumentCreated(object sender, ValueEventArgs<IComObject<ID2D1SvgDocument>?> e) => SvgDocumentCreated?.Invoke(this, e);

    /// <inheritdoc/>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == DocumentProperty)
        {
            Interlocked.Exchange(ref _documentBuffer, null)?.Dispose();
        }
        return true;
    }

    /// <summary>
    /// Computes the rectangle into which the SVG content will be rendered, honoring alignment,
    /// <see cref="Stretch"/>, <see cref="StretchDirection"/>, and <see cref="Visual.RelativeRenderRect"/>.
    /// </summary>
    public virtual D2D_RECT_F GetDestinationRectangle() => Image.GetDestinationRectangle(
                RenderSize,
                HorizontalAlignment,
                VerticalAlignment,
                Stretch,
                StretchDirection,
                RelativeRenderRect);

    /// <inheritdoc/>
    protected internal override void RenderCore(RenderContext context)
    {
        base.RenderCore(context);
        var doc = Document;
        if (doc == null)
            return;

        var dc = context.DeviceContext.As<ID2D1DeviceContext5>(false);
        if (dc == null)
            return;

        var rc = GetDestinationRectangle();

        //
        // note we cannot use D2D transforms on a SVG in a Direct Composition context, like this:
        //
        //    context.DeviceContext.Object.SetTransform(ref xf);
        //
        // as this causes strange flickering issues
        // so we have to create an svg document each render time (mostly due to resize)
        // hence the possible buffer feature
        //

        IComObject<ID2D1SvgDocument>? svg = null;
        if (BufferStream)
        {
            if (_documentBuffer == null)
            {
                using var stream = doc.GetReadStream();
                if (stream != null)
                {
#if NETFRAMEWORK
                    _documentBuffer = new DirectN.UnmanagedMemoryStream(stream);
#else
                    _documentBuffer = new DirectN.Extensions.Utilities.UnmanagedMemoryStream(stream);
#endif
                }
            }

            if (_documentBuffer?.Length > 0)
            {
                _documentBuffer.Position = 0;
                svg = dc.CreateSvgDocument(_documentBuffer, rc.Size);
            }
        }
        else
        {
            using var stream = doc.GetReadStream();
            if (stream != null)
            {
                svg = dc.CreateSvgDocument(new ManagedIStream(stream), rc.Size);
            }
        }

        OnSvgDocumentCreated(this, new ValueEventArgs<IComObject<ID2D1SvgDocument>?>(svg));
        if (svg == null)
            return;

#if NETFRAMEWORK
        dc.DrawSvgDocument(svg.Object);
#else
        dc.DrawSvgDocument(svg);
#endif
    }

    /// <summary>
    /// Releases resources used by the control.
    /// </summary>
    /// <param name="disposing">
    /// true to dispose managed state (clears and disposes the buffered stream);
    /// false when called from a finalizer (no managed objects are touched).
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                Interlocked.Exchange(ref _documentBuffer, null)?.Dispose();
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            _disposedValue = true;
        }
    }

    /// <summary>
    /// Disposes the instance and suppresses finalization.
    /// </summary>
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
}
