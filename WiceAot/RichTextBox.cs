namespace Wice;

/// <summary>
/// A visual Rich Text Box that renders text content using a COM-backed text services host.
/// </summary>
public partial class RichTextBox : RenderVisual, IDisposable
{
    private bool _disposedValue;
    private D2D_SIZE_F _maxConstraintSize = new(ushort.MaxValue, ushort.MaxValue);
    private TXTNATURALSIZE _naturalSize = TXTNATURALSIZE.TXTNS_FITTOCONTENT2;
    private float _zoomFactor = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="RichTextBox"/> class.
    /// </summary>
    /// <param name="generator">
    /// The text services generator to use. When <see cref="TextServicesGenerator.Default"/>,
    /// the default generator is resolved via <see cref="GetDefaultTextServicesGenerator"/>.
    /// </param>
    public RichTextBox(TextServicesGenerator generator = TextServicesGenerator.Default)
    {
        if (generator == TextServicesGenerator.Default)
        {
            generator = GetDefaultTextServicesGenerator();
        }

        Generator = generator;

#if NETFRAMEWORK
        _host = new TextHost(generator)
        {
            TextColor = 0
        };
#else
        _host = CreateTextHost(generator, this);
#endif
        if (_host == null)
            throw new InvalidOperationException("Text host could not be created. Ensure the chosen generator is supported.");

        BackgroundColor = D3DCOLORVALUE.Transparent;
    }

#if NETFRAMEWORK
    private readonly TextHost _host;

    /// <summary>
    /// Gets the underlying host document (dynamic due to differing generator implementations).
    /// </summary>
    [Category(CategoryLive)]
    public dynamic Document => _host?.Document;

    /// <summary>
    /// Gets the version string of the active text services generator.
    /// </summary>
    [Category(CategoryBehavior)]
    public string GeneratorVersion => Document.Generator;
#else
    private readonly RichTextBoxTextHost _host;

    /// <summary>
    /// Gets the underlying text host instance.
    /// </summary>
    public TextHost Host => _host;

    /// <summary>
    /// Creates the text host used by this <see cref="RichTextBox"/>.
    /// </summary>
    /// <param name="generator">The text services generator to use.</param>
    /// <param name="richTextBox">The owning <see cref="RichTextBox"/>.</param>
    /// <returns>A concrete <see cref="RichTextBoxTextHost"/> instance.</returns>
    protected virtual RichTextBoxTextHost CreateTextHost(TextServicesGenerator generator, RichTextBox richTextBox) => new(generator, richTextBox);

    /// <summary>
    /// Text host implementation bound to a <see cref="RichTextBox"/> with DX-based layout/sizing helpers.
    /// </summary>
    [System.Runtime.InteropServices.Marshalling.GeneratedComClass]
    protected partial class RichTextBoxTextHost : TextHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RichTextBoxTextHost"/> class.
        /// </summary>
        /// <param name="generator">The text services generator to use.</param>
        /// <param name="richTextBox">The owning <see cref="RichTextBox"/>.</param>
        public RichTextBoxTextHost(TextServicesGenerator generator, RichTextBox richTextBox) : base(generator)
        {
            ExceptionExtensions.ThrowIfNull(richTextBox, nameof(richTextBox));
            RichTextBox = richTextBox;
            RichTextBox.DoWhenAttachedToComposition(() =>
            {
                WindowHandle = richTextBox.Window?.Handle ?? 0;
            });
        }

        /// <summary>
        /// Gets the owning <see cref="RichTextBox"/>.
        /// </summary>
        public RichTextBox RichTextBox { get; }

        /// <inheritdoc/>
        public override unsafe HRESULT TxGetExtent(nint lpExtent)
        {
            if (lpExtent == 0)
                return Constants.E_INVALIDARG;

            var dpi = DpiUtilities.GetDpiForWindow(WindowHandle).width;
            var value = ClientRect.Size;
            value.cx = value.cx.PixelToHiMetric(dpi);
            value.cy = value.cy.PixelToHiMetric(dpi);
            *(SIZE*)lpExtent = value;
            //Trace("lpExtent: " + value);
            return Constants.S_OK;
        }

        /// <inheritdoc/>
        public override uint TxGetSysColor(SYS_COLOR_INDEX nIndex) => Functions.GetSysColor(nIndex);

        /// <inheritdoc/>
        public unsafe override bool TxClientToScreen(nint lppt)
        {
            if (lppt == 0 || RichTextBox == null)
                return false;

            var arr = RichTextBox.AbsoluteRenderRect;
            if (arr.IsInvalid)
                return false;

            var window = RichTextBox.Window;
            if (window == null)
                return false;

            var pt = *(POINT*)lppt;
            pt = new POINT(pt.x + arr.left, pt.y - arr.top);
            pt = window.ClientToScreen(pt);
            *(POINT*)lppt = pt;
            return true;
        }

        /// <inheritdoc/>
        public unsafe override bool TxScreenToClient(nint lppt)
        {
            if (lppt == 0 || RichTextBox == null)
                return false;

            var window = RichTextBox.Window;
            if (window == null)
                return false;

            var pt = *(POINT*)lppt;
            pt = window.ScreenToClient(pt);
            var pos = RichTextBox.GetRelativePosition(pt.x, pt.y);
            *(POINT*)lppt = pos;
            return true;
        }

        /// <inheritdoc/>
        public unsafe override HRESULT TxGetWindowStyles(nint pdwStyle, nint pdwExStyle)
        {
            var window = RichTextBox?.Window;
            if (window == null)
                return Constants.E_FAIL;

            if (pdwStyle != 0)
            {
                *(WINDOW_STYLE*)pdwStyle = window.Style;
            }

            if (pdwExStyle != 0)
            {
                *(WINDOW_EX_STYLE*)pdwExStyle = window.ExtendedStyle;
            }
            return WiceCommons.S_OK;
        }

        /// <inheritdoc/>
        public override void TxViewChange(BOOL fUpdate)
        {
            RichTextBox?.Invalidate(VisualPropertyInvalidateModes.Render);
        }

        /// <inheritdoc/>
        public override unsafe void TxInvalidateRect(nint prc, BOOL fMode)
        {
            RichTextBox?.Invalidate(VisualPropertyInvalidateModes.Render);
        }
    }

    /// <summary>
    /// Gets the underlying COM text document interface when available.
    /// </summary>
    [Category(CategoryLive)]
    public IComObject<ITextDocument2>? Document => _host?.Document;

    /// <summary>
    /// Gets the version string of the active text services generator.
    /// </summary>
    [Category(CategoryBehavior)]
    public string GeneratorVersion => _host?.Generator ?? string.Empty;

#endif

    /// <summary>
    /// Gets the configured text services generator used by this control.
    /// </summary>
    [Category(CategoryBehavior)]
    public TextServicesGenerator Generator { get; }

    /// <summary>
    /// Gets or sets the default text color used by the host.
    /// </summary>
    [Category(CategoryRender)]
    public virtual D3DCOLORVALUE TextColor
    {
        get => TextHost.ToColor((_host?.TextColor).GetValueOrDefault());
        set
        {
            var host = _host;
            if (host == null)
                return;

            OnPropertyChanging();
            host.TextColor = TextHost.ToColor(value);
            Invalidate(VisualPropertyInvalidateModes.Render);
        }
    }

    /// <summary>
    /// Gets or sets layout-related options for the underlying text host (word-wrap, selection, etc.).
    /// </summary>
    [Category(CategoryLayout)]
    public virtual TextHostOptions Options
    {
#if NETFRAMEWORK
        get => (_host?.Options).GetValueOrDefault();
#else
        get => _host?.Options ?? TextHostOptions.Default;
#endif
        set
        {
            var host = _host;
            if (host == null)
                return;

            OnPropertyChanging();
            host.Options = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the plain text content.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual string Text
    {
        get => _host?.Text ?? string.Empty;
        set
        {
            var host = _host;
            if (host == null)
                return;

            OnPropertyChanging();
            host.Text = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the RTF content.
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual string RtfText
    {
        get => _host?.RtfText ?? string.Empty;
        set
        {
            var host = _host;
            if (host == null)
                return;

            OnPropertyChanging();
            host.RtfText = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the HTML content (only supported with the Office generator).
    /// </summary>
    [Category(CategoryBehavior)]
    public virtual string HtmlText
    {
        get => _host?.HtmlText ?? string.Empty;
        set
        {
            var host = _host;
            if (host == null)
                return;

            OnPropertyChanging();
            host.HtmlText = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the default font family name (e.g., "Calibri").
    /// </summary>
    [Category(CategoryLayout)]
    public virtual string FontName
    {
        get => _host?.FaceName ?? string.Empty;
        set
        {
            var host = _host;
            if (host == null)
                return;

            OnPropertyChanging();
            host.FaceName = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the default font size in DIPs.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual int FontSize
    {
        get => _host?.Height ?? 0;
        set
        {
            var host = _host;
            if (host == null)
                return;

            OnPropertyChanging();
            host.Height = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets a uniform zoom factor applied during measure/arrange/render, useful for HiDPI scaling scenarios.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual float ZoomFactor
    {
        get => _zoomFactor;
        set
        {
            if (_zoomFactor == value)
                return;

            OnPropertyChanging();
            _zoomFactor = value;
            Invalidate(VisualPropertyInvalidateModes.Measure);
        }
    }

    /// <summary>
    /// Gets or sets the natural size computation mode used to measure text.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual TXTNATURALSIZE NaturalSize
    {
        get => _naturalSize;
        set
        {
            if (_naturalSize == value)
                return;

            OnPropertyChanging();
            _naturalSize = value;
            Invalidate(VisualPropertyInvalidateModes.Measure);
        }
    }

    /// <summary>
    /// Gets or sets a maximum constraint used during measure, protecting the host from invalid values.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual D2D_SIZE_F MaxConstraintSize
    {
        get => _maxConstraintSize;
        set
        {
            if (_maxConstraintSize == value)
                return;

            OnPropertyChanging();
            _maxConstraintSize = value;
            Invalidate(VisualPropertyInvalidateModes.Measure);
        }
    }

    /// <summary>
    /// Gets or sets the default font weight.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual ushort FontWeight
    {
#if NETFRAMEWORK
        get => (ushort)(_host?.Weight ?? 0);
#else
        get => _host?.Weight ?? 0;
#endif
        set
        {
            var host = _host;
            if (host == null)
                return;

            OnPropertyChanging();
#if NETFRAMEWORK
            host.Weight = (short)value;
#else
            host.Weight = value;
#endif
            Invalidate();
        }
    }

#if !NETFRAMEWORK
    /// <summary>
    /// Gets or sets the background style (opaque or transparent) used by the host.
    /// </summary>
    [Category(CategoryLayout)]
    public virtual TXTBACKSTYLE BackStyle
    {
        get => _host?.BackStyle ?? TXTBACKSTYLE.TXTBACK_TRANSPARENT;
        set
        {
            var host = _host;
            if (host == null)
                return;

            OnPropertyChanging();
            host.BackStyle = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets text using TOM flags.
    /// </summary>
    /// <param name="flags">TOM flags controlling which text to retrieve.</param>
    /// <returns>The requested text, or null if the host is unavailable.</returns>
    public virtual string? GetText(tomConstants flags) => _host?.GetText(flags);

    /// <summary>
    /// Sets text using TOM flags.
    /// </summary>
    /// <param name="flags">TOM flags controlling which text to set.</param>
    /// <param name="text">The text to set.</param>
    public virtual void SetText(tomConstants flags, string text)
    {
        if (_host == null)
            throw new InvalidOperationException("Text host is not initialized.");

        OnPropertyChanging();
        _host.SetText(flags, text);
        OnPropertyChanged(nameof(Text));
        CheckRunningAsMainThread();
        base.Invalidate(VisualPropertyInvalidateModes.Measure);
    }
#endif

    /// <summary>
    /// Sends a message to the underlying text services host.
    /// </summary>
    /// <param name="msg">The message identifier.</param>
    /// <param name="lParam">Message LParam.</param>
    /// <returns>HRESULT indicating success or failure.</returns>
    public HRESULT SendMessage(uint msg, LPARAM lParam) => SendMessage(msg, 0, lParam);

    /// <summary>
    /// Sends a message to the underlying text services host.
    /// </summary>
    /// <param name="msg">The message identifier.</param>
    /// <param name="wParam">Message WParam.</param>
    /// <returns>HRESULT indicating success or failure.</returns>
    public HRESULT SendMessage(uint msg, WPARAM wParam) => SendMessage(msg, wParam, 0);

    /// <summary>
    /// Sends a message to the underlying text services host.
    /// </summary>
    /// <param name="msg">The message identifier.</param>
    /// <param name="wParam">Message WParam.</param>
    /// <param name="lParam">Message LParam.</param>
    /// <returns>HRESULT indicating success or failure.</returns>
    public HRESULT SendMessage(uint msg, WPARAM wParam, LPARAM lParam) => SendMessage(msg, wParam, lParam, out _);

    /// <summary>
    /// Sends a message to the underlying text services host and retrieves the result.
    /// </summary>
    /// <param name="msg">The message identifier.</param>
    /// <param name="wParam">Message WParam.</param>
    /// <param name="lParam">Message LParam.</param>
    /// <param name="result">Receives the message result.</param>
    /// <returns>HRESULT indicating success or failure.</returns>
    public HRESULT SendMessage(uint msg, WPARAM wParam, LPARAM lParam, out LRESULT result
    )
    {
        result = default;
        if (_host == null)
            return WiceCommons.E_FAIL;

#if NETFRAMEWORK
        var hr = _host.Services.TxSendMessage((int)msg, wParam, lParam, out var res);
        result = res;
        return hr;
#else
        return _host.SendMessage(msg, wParam, lParam, out result);
#endif
    }

    /// <summary>
    /// Invalidates the visual and notifies property change hooks.
    /// </summary>
    /// <param name="modes">Which invalidation modes to trigger (defaults to Measure).</param>
    /// <param name="propertyName">The property name triggering the invalidation.</param>
    protected virtual void Invalidate(VisualPropertyInvalidateModes modes = VisualPropertyInvalidateModes.Measure, [CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(propertyName);
        CheckRunningAsMainThread();
        base.Invalidate(modes);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToComposition(object? sender, EventArgs e)
    {
        base.OnAttachedToComposition(sender, e);
        OnThemeDpiEvent(Window, ThemeDpiEventArgs.FromWindow(Window));
        Window!.ThemeDpiEvent += OnThemeDpiEvent;
    }

    /// <inheritdoc/>
    protected override void OnDetachingFromComposition(object? sender, EventArgs e)
    {
        base.OnDetachingFromComposition(sender, e);
        Window!.ThemeDpiEvent -= OnThemeDpiEvent;
    }

    /// <summary>
    /// Responds to DPI changes by scaling font size and updating <see cref="ZoomFactor"/>.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">DPI change data.</param>
    protected virtual void OnThemeDpiEvent(object? sender, ThemeDpiEventArgs e)
    {
        FontSize = UIExtensions.DpiScale(FontSize, e.OldDpi, e.NewDpi);
        ZoomFactor = Window!.Dpi / (float)WiceCommons.USER_DEFAULT_SCREEN_DPI;
    }

    /// <inheritdoc/>
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
    {
        var host = _host;
        if (host == null)
            return base.MeasureCore(constraint);

        var padding = Padding;
        var leftPadding = padding.left.IsSet() && padding.left > 0;
        if (leftPadding && constraint.width.IsSet())
        {
            constraint.width = Math.Max(0, constraint.width - padding.left);
        }

        var topPadding = padding.top.IsSet() && padding.top > 0;
        if (topPadding && constraint.height.IsSet())
        {
            constraint.height = Math.Max(0, constraint.height - padding.top);
        }

        var rightPadding = padding.right.IsSet() && padding.right > 0;
        if (rightPadding && constraint.width.IsSet())
        {
            constraint.width = Math.Max(0, constraint.width - padding.right);
        }

        var bottomPadding = padding.bottom.IsSet() && padding.bottom > 0;
        if (bottomPadding && constraint.height.IsSet())
        {
            constraint.height = Math.Max(0, constraint.height - padding.bottom);
        }

        // not sure why but if we don't set these to ushort.MaxValue, the text host will not work properly
        var max = MaxConstraintSize;
        if (constraint.width >= max.width)
        {
            constraint.width = max.width;
        }

        if (constraint.height >= max.height)
        {
            constraint.height = max.height;
        }

        // divide by zoom factor when asking rtb to measure
        var zf = ZoomFactor;
        if (zf != 0)
        {
            constraint.width /= zf;
            constraint.height /= zf;
        }

        var size = host.GetNaturalSize(NaturalSize, constraint).ToD2D_SIZE_F();

        // multiply by zoom factor what rtb has given us
        if (zf != 0)
        {
            size.width *= zf;
            size.height *= zf;
        }

        if (leftPadding)
        {
            size.width += padding.left;
        }

        if (topPadding)
        {
            size.height += padding.top;
        }

        if (rightPadding)
        {
            size.width += padding.right;
        }

        if (bottomPadding)
        {
            size.height += padding.bottom;
        }

        return size;
    }

    private RECT GetRect(D2D_RECT_F finalRect)
    {
        var padding = Padding;
        var rc = new D2D_RECT_F();
        if (padding.left.IsSet() && padding.left > 0)
        {
            rc.left = padding.left;
        }

        if (padding.top.IsSet() && padding.top > 0)
        {
            rc.top = padding.top;
        }

        if (padding.right.IsSet() && padding.right > 0)
        {
            rc.Width = Math.Max(0, finalRect.Width - padding.right - rc.left);
        }
        else
        {
            rc.Width = finalRect.Width;
        }

        if (padding.bottom.IsSet() && padding.bottom > 0)
        {
            rc.Height = Math.Max(0, finalRect.Height - padding.bottom - rc.top);
        }
        else
        {
            rc.Height = finalRect.Height;
        }

        return rc.ToRECT();
    }

    /// <inheritdoc/>
    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        base.ArrangeCore(finalRect);
        var host = _host;
        if (host == null)
            return;

        var rc = GetRect(finalRect);
        host.Activate(rc);
    }

    /// <inheritdoc/>
    protected internal override void RenderCore(RenderContext context)
    {
        base.RenderCore(context);
        var host = _host;
        if (host == null)
            return;

        var rc = GetRect(ArrangedRect);
        var rr = RelativeRenderRect;
        var urc = rc;
        urc.top = -(int)rr.top;

        var vp = GetViewerParent();
        if (vp != null)
        {
            var ar = ((Visual)vp).AbsoluteRenderRect;
            if (ar.IsValid)
            {
                if (urc.Width > ar.Width)
                {
                    urc.Width = (int)ar.Width;
                }

                if (urc.Height > ar.Height)
                {
                    urc.Height = (int)ar.Height;
                }
            }
        }

        host.Draw(context.DeviceContext.Object, rc, urc);
    }

    private IViewerParent? GetViewerParent() => Parent is Viewer viewer ? viewer.Parent as ScrollViewer : null;

    /// <inheritdoc/>
    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == BackgroundColorProperty)
        {
            var host = _host;
            host?.BackColor = TextHost.ToColor((D3DCOLORVALUE)value!);
            return true;
        }

        return true;
    }

    /// <summary>
    /// Disposes the underlying host. Must be called on the same thread that created the host.
    /// </summary>
    /// <param name="disposing">true to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                try
                {
                    _host?.Dispose();
                }
                catch
                {
                    // continue. should be removed when DirectN is upgraded
                }
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            _disposedValue = true;
        }
    }

    /// <summary>
    /// Disposes the visual and suppresses finalization.
    /// </summary>
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }

    // allow command line change
#if NETFRAMEWORK
    /// <summary>
    /// Gets the default text services generator, optionally overridden by command line argument "TextServicesGenerator".
    /// </summary>
    public static TextServicesGenerator GetDefaultTextServicesGenerator() => CommandLine.GetArgument(nameof(TextServicesGenerator), TextServicesGenerator.Default);
#else
    /// <summary>
    /// Gets the default text services generator, optionally overridden by command line argument "TextServicesGenerator".
    /// </summary>
    public static TextServicesGenerator GetDefaultTextServicesGenerator() => CommandLine.Current.GetArgument(nameof(TextServicesGenerator), TextServicesGenerator.Default);
#endif

    private static readonly Lazy<string> _defaultTextServicesGeneratorVersion = new(GetDefaultTextServicesGeneratorVersion);

    /// <summary>
    /// Creates a temporary <see cref="RichTextBox"/> to query the generator version string.
    /// </summary>
    /// <returns>The generator version string.</returns>
    public static string GetDefaultTextServicesGeneratorVersion()
    {
        using var rtb = new RichTextBox();
        return rtb.GeneratorVersion;
    }

    /// <summary>
    /// Gets the default text services generator version, computed once lazily.
    /// </summary>
    public static string DefaultTextServicesGeneratorVersion { get; } = _defaultTextServicesGeneratorVersion.Value;
}
