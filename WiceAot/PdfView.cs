using Windows.Data.Pdf;
using Windows.Storage;

namespace Wice;

public partial class PdfView : RenderVisual, IDisposable
{
    public static VisualProperty SourceFilePathProperty { get; } = VisualProperty.Add<string>(typeof(PdfView), nameof(SourceFilePath), VisualPropertyInvalidateModes.Render, changed: OnSourceChanged);
    public static VisualProperty SourceStreamProperty { get; } = VisualProperty.Add<Stream>(typeof(PdfView), nameof(SourceStream), VisualPropertyInvalidateModes.Render, changed: OnSourceChanged);
    public static VisualProperty PasswordProperty { get; } = VisualProperty.Add<string>(typeof(PdfView), nameof(Password), VisualPropertyInvalidateModes.Render, changed: OnSourceChanged);
    public static VisualProperty CurrentPageProperty { get; } = VisualProperty.Add<int>(typeof(PdfView), nameof(CurrentPage), VisualPropertyInvalidateModes.Render, changing: OnPageChanging, changed: OnPageChanged);
    public static VisualProperty IgnoreHighContrastProperty { get; } = VisualProperty.Add<bool>(typeof(PdfView), nameof(IgnoreHighContrast), VisualPropertyInvalidateModes.Render);
    public static VisualProperty StretchProperty { get; } = VisualProperty.Add(typeof(PdfView), nameof(Stretch), VisualPropertyInvalidateModes.Measure, Stretch.Uniform);
    public static VisualProperty StretchDirectionProperty { get; } = VisualProperty.Add(typeof(PdfView), nameof(StretchDirection), VisualPropertyInvalidateModes.Measure, StretchDirection.Both);

    private static void OnSourceChanged(BaseObject obj, object? newValue, object? oldValue) => ((PdfView)obj).OnSourceChanged();
    private static void OnPageChanged(BaseObject obj, object? newValue, object? oldValue) => ((PdfView)obj).OnPageChanged();
    private static bool OnPageChanging(BaseObject obj, object? newValue, object? oldValue) => ((PdfView)obj).OnPageChanging((int)newValue!);

    public event EventHandler<EventArgs>? DocumentLoaded;
    public event EventHandler<EventArgs>? DocumentLoadError;
    public event EventHandler<EventArgs>? DocumentDisposed;
    public event EventHandler<EventArgs>? PageChanged;

    private PdfDocument? _pdfDocument;
    private PdfPage? _pdfPage;
    private ComObject<IPdfRendererNative>? _pdfRendererNative;
    private bool _disposedValue;

    [Category(CategoryLayout)]
    public virtual string SourceFilePath { get => (string?)GetPropertyValue(SourceFilePathProperty) ?? string.Empty; set => SetPropertyValue(SourceFilePathProperty, value); }

    [Category(CategoryLayout)]
    public virtual Stream? SourceStream { get => (Stream?)GetPropertyValue(SourceStreamProperty); set => SetPropertyValue(SourceStreamProperty, value); }

    [Category(CategoryLayout)]
    public virtual string? Password { get => (string?)GetPropertyValue(PasswordProperty); set => SetPropertyValue(PasswordProperty, value); }

    [Category(CategoryLayout)]
    public virtual int CurrentPage { get => (int)GetPropertyValue(CurrentPageProperty)!; set => SetPropertyValue(CurrentPageProperty, value); }

    [Category(CategoryLayout)]
    public virtual bool IgnoreHighContrast { get => (bool)GetPropertyValue(IgnoreHighContrastProperty)!; set => SetPropertyValue(IgnoreHighContrastProperty, value); }

    [Category(CategoryLayout)]
    public Stretch Stretch { get => (Stretch)GetPropertyValue(StretchProperty)!; set => SetPropertyValue(StretchProperty, value); }

    [Category(CategoryLayout)]
    public StretchDirection StretchDirection { get => (StretchDirection)GetPropertyValue(StretchDirectionProperty)!; set => SetPropertyValue(StretchDirectionProperty, value); }

    protected override bool FallbackToTransparentBackground => true;
    public virtual PdfDocument? Document => _pdfDocument;
    public virtual PdfPage? Page => _pdfPage;
    public virtual Exception? LoadError { get; protected set; }
    public uint PagesCount => _pdfDocument?.PageCount ?? 0;

    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint) => GetPageSize(constraint);

    protected virtual D2D_SIZE_F GetPageSize(D2D_SIZE_F constraint)
    {
        var width = 0f;
        var height = 0f;

        var page = EnsurePage();
        if (page != null)
        {
            var pageSize = page.Size;
            var size = new D2D_SIZE_F(pageSize.Width, pageSize.Height);
            width = size.width;
            height = size.height;
            var stretch = Stretch;
            if (stretch != Stretch.None)
            {
                var factor = GetScaleFactor(constraint, new D2D_SIZE_F(size.width, size.height), stretch, StretchDirection);
                return new D2D_SIZE_F(width * factor.width, height * factor.height);
            }
        }
        return new D2D_SIZE_F(width, height);
    }

    protected virtual void OnSourceChanged()
    {
        Dispose(true);
        _ = LoadAsync();
    }

    protected virtual bool OnPageChanging(int newPage) => newPage >= 0 && newPage < PagesCount;
    protected virtual void OnPageChanged()
    {
        Interlocked.Exchange(ref _pdfPage, null)?.Dispose();
    }

    protected async Task LoadAsync()
    {
        Dispose();
        try
        {
            var password = Password;
            var path = SourceFilePath.Nullify();
            if (path != null)
            {
                if (!System.IO.Path.IsPathRooted(path))
                {
                    path = System.IO.Path.GetFullPath(path);
                }

                var file = await StorageFile.GetFileFromPathAsync(path);
                if (password != null)
                {
                    _pdfDocument = await PdfDocument.LoadFromFileAsync(file, password);
                }
                else
                {
                    _pdfDocument = await PdfDocument.LoadFromFileAsync(file);
                }

                OnDocumentLoaded(this, EventArgs.Empty);
                Invalidate(VisualPropertyInvalidateModes.Render);
                return;
            }

            var stream = SourceStream;
            if (stream != null)
            {
                if (password != null)
                {
                    _pdfDocument = await PdfDocument.LoadFromStreamAsync(stream.AsRandomAccessStream(), password);
                }
                else
                {
                    _pdfDocument = await PdfDocument.LoadFromStreamAsync(stream.AsRandomAccessStream());
                }

                OnDocumentLoaded(this, EventArgs.Empty);
                Invalidate(VisualPropertyInvalidateModes.Render);
                return;
            }

            // raised even if no document is loaded
            OnDocumentLoaded(this, EventArgs.Empty);
            Invalidate(VisualPropertyInvalidateModes.Render);
        }
        catch (Exception ex)
        {
            if (OnLoadError(ex))
                throw;
        }
    }

    protected virtual bool OnLoadError(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        LoadError = exception;
        OnDocumentLoadError(this, EventArgs.Empty);
        return true;
    }

    protected virtual PdfPage? EnsurePage()
    {
        var doc = _pdfDocument;
        if (doc == null)
            return null;

        var page = CurrentPage;
        if (page < 0 || page >= doc.PageCount)
            return null;

        _pdfPage = doc.GetPage((uint)page);
        OnPageChanged(this, EventArgs.Empty);
        return _pdfPage;
    }

    protected virtual IComObject<IPdfRendererNative>? EnsureRenderer()
    {
        var renderer = _pdfRendererNative;
        if (renderer != null && !renderer.IsDisposed)
            return renderer;

        var device = Window?.D3D11Device.As<IDXGIDevice>(false);
        if (device == null)
            return null;

        Functions.PdfCreateRenderer(device.Object, out var obj).ThrowOnError();
        _pdfRendererNative = new ComObject<IPdfRendererNative>(obj);
        return _pdfRendererNative;
    }

    public virtual D2D_RECT_F GetDestinationRectangle()
    {
        var page = EnsurePage();
        if (page == null)
            return D2D_RECT_F.Zero;

        var pageSize = page.Size;
        var size = new D2D_SIZE_F(pageSize.Width, pageSize.Height);
        return Image.GetDestinationRectangle(
                size,
                HorizontalAlignment,
                VerticalAlignment,
                Stretch,
                StretchDirection,
                RelativeRenderRect);
    }

    protected unsafe internal override void RenderCore(RenderContext context)
    {
        if (LoadError != null)
            throw LoadError;

        base.RenderCore(context);
        var doc = _pdfDocument;
        if (doc == null)
            return;

        var renderer = EnsureRenderer();
        if (renderer == null)
            return;

        var page = EnsurePage();
        if (page == null)
            return;

        var pageUnk = ((IWinRTObject)page).NativeObject.ThisPtr; // no AddRef needed

        var rc = GetDestinationRectangle();
        var renderParams = new PDF_RENDER_PARAMS
        {
            BackgroundColor = D3DCOLORVALUE.White,
            DestinationWidth = (uint)rc.Width,
            DestinationHeight = (uint)rc.Height,
            IgnoreHighContrast = IgnoreHighContrast,
        };
        renderer.Object.RenderPageToDeviceContext(pageUnk, context.DeviceContext.Object, (nint)(&renderParams)).ThrowOnError();
    }

    protected virtual void OnDocumentDisposed(object sender, EventArgs args) => DocumentDisposed?.Invoke(sender, args);
    protected virtual void OnDocumentLoaded(object sender, EventArgs args) => DocumentLoaded?.Invoke(sender, args);
    protected virtual void OnPageChanged(object sender, EventArgs args) => PageChanged?.Invoke(sender, args);
    protected virtual void OnDocumentLoadError(object sender, EventArgs args) => DocumentLoadError?.Invoke(sender, args);

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                Interlocked.Exchange(ref _pdfRendererNative, null)?.Dispose();
                Interlocked.Exchange(ref _pdfPage, null)?.Dispose();
                var doc = Interlocked.Exchange(ref _pdfDocument, null);
                if (doc != null)
                {
                    OnDocumentDisposed(this, EventArgs.Empty);
                }
                CurrentPage = 0;
                LoadError = null;
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            _disposedValue = true;
        }
    }

    ~PdfView() { Dispose(disposing: false); }
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
}
