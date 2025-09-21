using Windows.Data.Pdf;
using Windows.Storage;

namespace Wice;

/// <summary>
/// A visual that renders a PDF page using Windows.Data.Pdf and the native IPdfRenderer.
/// </summary>
public partial class PdfView : RenderVisual, IDisposable
{
    /// <summary>
    /// Gets the visual property that stores <see cref="SourceFilePath"/>.
    /// </summary>
    public static VisualProperty SourceFilePathProperty { get; } = VisualProperty.Add<string>(typeof(PdfView), nameof(SourceFilePath), VisualPropertyInvalidateModes.Render, changed: OnSourceChanged);

    /// <summary>
    /// Gets the visual property that stores <see cref="SourceStream"/>.
    /// </summary>
    public static VisualProperty SourceStreamProperty { get; } = VisualProperty.Add<Stream>(typeof(PdfView), nameof(SourceStream), VisualPropertyInvalidateModes.Render, changed: OnSourceChanged);

    /// <summary>
    /// Gets the visual property that stores <see cref="Password"/>.
    /// </summary>
    public static VisualProperty PasswordProperty { get; } = VisualProperty.Add<string>(typeof(PdfView), nameof(Password), VisualPropertyInvalidateModes.Render, changed: OnSourceChanged);

    /// <summary>
    /// Gets the visual property that stores <see cref="CurrentPage"/>.
    /// </summary>
    public static VisualProperty CurrentPageProperty { get; } = VisualProperty.Add<int>(typeof(PdfView), nameof(CurrentPage), VisualPropertyInvalidateModes.Render, changing: OnPageChanging, changed: OnPageChanged);

    /// <summary>
    /// Gets the visual property that stores <see cref="IgnoreHighContrast"/>.
    /// </summary>
    public static VisualProperty IgnoreHighContrastProperty { get; } = VisualProperty.Add<bool>(typeof(PdfView), nameof(IgnoreHighContrast), VisualPropertyInvalidateModes.Render);

    /// <summary>
    /// Gets the visual property that stores <see cref="Stretch"/>.
    /// </summary>
    public static VisualProperty StretchProperty { get; } = VisualProperty.Add(typeof(PdfView), nameof(Stretch), VisualPropertyInvalidateModes.Measure, Stretch.Uniform);

    /// <summary>
    /// Gets the visual property that stores <see cref="StretchDirection"/>.
    /// </summary>
    public static VisualProperty StretchDirectionProperty { get; } = VisualProperty.Add(typeof(PdfView), nameof(StretchDirection), VisualPropertyInvalidateModes.Measure, StretchDirection.Both);

    private static void OnSourceChanged(BaseObject obj, object? newValue, object? oldValue) => ((PdfView)obj).OnSourceChanged();
    private static void OnPageChanged(BaseObject obj, object? newValue, object? oldValue) => ((PdfView)obj).OnPageChanged();
    private static bool OnPageChanging(BaseObject obj, object? newValue, object? oldValue) => ((PdfView)obj).OnPageChanging((int)newValue!);

    /// <summary>
    /// Occurs after the document loading routine completes (regardless of success).
    /// </summary>
    public event EventHandler<EventArgs>? DocumentLoaded;

    /// <summary>
    /// Occurs when the document load operation fails with an exception.
    /// </summary>
    public event EventHandler<EventArgs>? DocumentLoadError;

    /// <summary>
    /// Occurs when the currently loaded document is disposed/released.
    /// </summary>
    public event EventHandler<EventArgs>? DocumentDisposed;

    /// <summary>
    /// Occurs after the current page is changed and the previous page object is disposed.
    /// </summary>
    public event EventHandler<EventArgs>? PageChanged;

    private PdfDocument? _pdfDocument;
    private PdfPage? _pdfPage;
    private ComObject<IPdfRendererNative>? _pdfRendererNative;

    /// <summary>
    /// Gets or sets a filesystem path to a PDF file. Setting this triggers a reload.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual string SourceFilePath { get => (string?)GetPropertyValue(SourceFilePathProperty) ?? string.Empty; set => SetPropertyValue(SourceFilePathProperty, value); }

    /// <summary>
    /// Gets or sets a managed stream containing PDF data. Setting this triggers a reload.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual Stream? SourceStream { get => (Stream?)GetPropertyValue(SourceStreamProperty); set => SetPropertyValue(SourceStreamProperty, value); }

    /// <summary>
    /// Gets or sets the password used to open encrypted PDF documents. Setting this triggers a reload.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual string? Password { get => (string?)GetPropertyValue(PasswordProperty); set => SetPropertyValue(PasswordProperty, value); }

    /// <summary>
    /// Gets or sets the zero-based current page index to render.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual int CurrentPage { get => (int)GetPropertyValue(CurrentPageProperty)!; set => SetPropertyValue(CurrentPageProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the PDF renderer should ignore system high-contrast.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual bool IgnoreHighContrast { get => (bool)GetPropertyValue(IgnoreHighContrastProperty)!; set => SetPropertyValue(IgnoreHighContrastProperty, value); }

    /// <summary>
    /// Gets or sets how to scale the page within the available space.
    /// </summary>
    [Category(CategoryLayout)]
    public Stretch Stretch { get => (Stretch)GetPropertyValue(StretchProperty)!; set => SetPropertyValue(StretchProperty, value); }

    /// <summary>
    /// Gets or sets how scaling is constrained (up/down/both) when applying <see cref="Stretch"/>.
    /// </summary>
    [Category(CategoryLayout)]
    public StretchDirection StretchDirection { get => (StretchDirection)GetPropertyValue(StretchDirectionProperty)!; set => SetPropertyValue(StretchDirectionProperty, value); }

    /// <inheritdoc/>
    protected override bool FallbackToTransparentBackground => true;

    /// <summary>
    /// Gets the currently loaded <see cref="PdfDocument"/>, if any.
    /// </summary>
    public virtual PdfDocument? Document => _pdfDocument;

    /// <summary>
    /// Gets the currently materialized <see cref="PdfPage"/> instance, if any.
    /// </summary>
    public virtual PdfPage? Page => _pdfPage;

    /// <summary>
    /// Gets the last load error, if any.
    /// </summary>
    public virtual Exception? LoadError { get; protected set; }

    /// <summary>
    /// Gets the number of pages in the loaded document, or 0 when no document is loaded.
    /// </summary>
    public uint PagesCount => _pdfDocument?.PageCount ?? 0;

    /// <inheritdoc/>
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint) => GetPageSize(constraint);

    /// <summary>
    /// Computes the desired size for the current page given an available size and the configured stretch settings.
    /// </summary>
    /// <param name="constraint">The available layout size.</param>
    /// <returns>The desired size.</returns>
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

    /// <summary>
    /// Handles changes to the source (file path/stream/password) by disposing current state and reloading.
    /// </summary>
    protected virtual void OnSourceChanged()
    {
        Dispose(true);
        _ = LoadAsync();
    }

    /// <summary>
    /// Validates the requested <see cref="CurrentPage"/> before it is applied.
    /// </summary>
    /// <param name="newPage">The zero-based page index.</param>
    /// <returns>true to accept; false to veto.</returns>
    protected virtual bool OnPageChanging(int newPage) => newPage >= 0 && newPage < PagesCount;

    /// <summary>
    /// Disposes the materialized <see cref="PdfPage"/> when the <see cref="CurrentPage"/> changes.
    /// </summary>
    protected virtual void OnPageChanged()
    {
        Interlocked.Exchange(ref _pdfPage, null)?.Dispose();
    }

    /// <summary>
    /// Asynchronously loads a PDF document from <see cref="SourceFilePath"/> or <see cref="SourceStream"/>.
    /// </summary>
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

    /// <summary>
    /// Handles a load error by storing it, raising <see cref="DocumentLoadError"/>, and deciding whether to rethrow.
    /// </summary>
    /// <param name="exception">The load exception.</param>
    /// <returns>true to rethrow the exception; false to swallow.</returns>
    protected virtual bool OnLoadError(Exception exception)
    {
        ExceptionExtensions.ThrowIfNull(exception, nameof(exception));
        LoadError = exception;
        OnDocumentLoadError(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// Ensures a <see cref="PdfPage"/> for the current <see cref="CurrentPage"/> is available.
    /// </summary>
    /// <returns>The page if available; otherwise null.</returns>
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

    /// <summary>
    /// Ensures the native PDF renderer is created and bound to the current window's D3D11 device.
    /// </summary>
    /// <returns>The renderer or null when no device/window is available.</returns>
    protected virtual IComObject<IPdfRendererNative>? EnsureRenderer()
    {
        var renderer = _pdfRendererNative;
        if (renderer != null && !renderer.IsDisposed)
            return renderer;

        var device = Window?.D3D11Device.As<IDXGIDevice>(false);
        if (device == null)
            return null;

#if NETFRAMEWORK
        WiceCommons.PdfCreateRenderer(device, out var obj).ThrowOnError();
#else
        WiceCommons.PdfCreateRenderer(device.Object, out var obj).ThrowOnError();
#endif
        _pdfRendererNative = new ComObject<IPdfRendererNative>(obj);
        return _pdfRendererNative;
    }

    /// <summary>
    /// Computes the destination rectangle for rendering the page within this visual, honoring stretch and alignment.
    /// </summary>
    /// <returns>The destination rectangle in device-independent pixels.</returns>
    public virtual D2D_RECT_F GetDestinationRectangle()
    {
        var page = EnsurePage();
        if (page == null)
            return new D2D_RECT_F();

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

    /// <summary>
    /// Renders the current PDF page to the given device context.
    /// </summary>
    /// <param name="context">The render context wrapping a Direct2D device context.</param>
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

#if NETFRAMEWORK
        var pageUnk = Marshal.GetIUnknownForObject(page);
#else
        var pageUnk = ((WinRT.IWinRTObject)page).NativeObject.ThisPtr; // no AddRef needed
#endif

        var rc = GetDestinationRectangle();
        var renderParams = new PDF_RENDER_PARAMS
        {
            BackgroundColor = D3DCOLORVALUE.White,
#if NETFRAMEWORK
            DestinationWidth = (int)rc.Width,
            DestinationHeight = (int)rc.Height,
#else
            DestinationWidth = (uint)rc.Width,
            DestinationHeight = (uint)rc.Height,
#endif
            IgnoreHighContrast = IgnoreHighContrast,
        };
        renderer.Object.RenderPageToDeviceContext(pageUnk, context.DeviceContext.Object, (nint)(&renderParams)).ThrowOnError();

#if NETFRAMEWORK
        Marshal.Release(pageUnk);
#endif
    }

    /// <summary>
    /// Raises <see cref="DocumentDisposed"/>.
    /// </summary>
    protected virtual void OnDocumentDisposed(object sender, EventArgs args) => DocumentDisposed?.Invoke(sender, args);

    /// <summary>
    /// Raises <see cref="DocumentLoaded"/>.
    /// </summary>
    protected virtual void OnDocumentLoaded(object sender, EventArgs args) => DocumentLoaded?.Invoke(sender, args);

    /// <summary>
    /// Raises <see cref="PageChanged"/>.
    /// </summary>
    protected virtual void OnPageChanged(object sender, EventArgs args) => PageChanged?.Invoke(sender, args);

    /// <summary>
    /// Raises <see cref="DocumentLoadError"/>.
    /// </summary>
    protected virtual void OnDocumentLoadError(object sender, EventArgs args) => DocumentLoadError?.Invoke(sender, args);

    /// <summary>
    /// Disposes managed state and resets the control to defaults.
    /// </summary>
    /// <param name="disposing">true if called from <see cref="Dispose()"/>; false if from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // dispose managed state (managed objects)
            Interlocked.Exchange(ref _pdfPage, null)?.Dispose();
            var doc = Interlocked.Exchange(ref _pdfDocument, null);
            if (doc != null)
            {
                OnDocumentDisposed(this, EventArgs.Empty);
            }
            CurrentPage = 0;
            LoadError = null;
            Interlocked.Exchange(ref _pdfRendererNative, null)?.Dispose();
        }
    }

    /// <summary>
    /// Releases resources used by this instance.
    /// </summary>
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
}
