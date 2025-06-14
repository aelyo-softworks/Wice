namespace Wice;

public partial class SvgImage : RenderVisual, IDisposable
{
    public static VisualProperty DocumentProperty { get; } = VisualProperty.Add<IReadStreamer>(typeof(SvgImage), nameof(Document), VisualPropertyInvalidateModes.Measure);
    public static VisualProperty StretchProperty { get; } = VisualProperty.Add(typeof(SvgImage), nameof(Stretch), VisualPropertyInvalidateModes.Measure, Stretch.Uniform);
    public static VisualProperty StretchDirectionProperty { get; } = VisualProperty.Add(typeof(SvgImage), nameof(StretchDirection), VisualPropertyInvalidateModes.Measure, StretchDirection.Both);

    public event EventHandler<ValueEventArgs<IComObject<ID2D1SvgDocument>?>>? SvgDocumentCreated;

    private bool _disposedValue;
#if NETFRAMEWORK
    private DirectN.UnmanagedMemoryStream _documentBuffer;
#else
    private DirectN.Extensions.Utilities.UnmanagedMemoryStream? _documentBuffer;
#endif
    private bool _bufferStream;

    public SvgImage()
    {
        BackgroundColor = D3DCOLORVALUE.Transparent;
        BufferStream = true;
    }

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

    [Category(CategoryBehavior)]
    public IReadStreamer Document { get => (IReadStreamer)GetPropertyValue(DocumentProperty)!; set => SetPropertyValue(DocumentProperty, value); }

    [Category(CategoryLayout)]
    public Stretch Stretch { get => (Stretch)GetPropertyValue(StretchProperty)!; set => SetPropertyValue(StretchProperty, value); }

    [Category(CategoryLayout)]
    public StretchDirection StretchDirection { get => (StretchDirection)GetPropertyValue(StretchDirectionProperty)!; set => SetPropertyValue(StretchDirectionProperty, value); }

    protected virtual void OnSvgDocumentCreated(object sender, ValueEventArgs<IComObject<ID2D1SvgDocument>?> e) => SvgDocumentCreated?.Invoke(this, e);

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

    public virtual D2D_RECT_F GetDestinationRectangle() => Image.GetDestinationRectangle(
                RenderSize,
                HorizontalAlignment,
                VerticalAlignment,
                Stretch,
                StretchDirection,
                RelativeRenderRect);

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

    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
}
