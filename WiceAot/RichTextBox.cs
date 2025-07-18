namespace Wice;

// this currently is only read-only
// note this visual sits on a COM object so we must dispose it on the same thread that created it
public partial class RichTextBox : RenderVisual, IDisposable
{
    private bool _disposedValue;
    private D2D_SIZE_F _maxConstraintSize = new(ushort.MaxValue, ushort.MaxValue);
    private TXTNATURALSIZE _naturalSize = TXTNATURALSIZE.TXTNS_FITTOCONTENT;

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

    [Category(CategoryLive)]
    public dynamic Document => _host?.Document;

    [Category(CategoryBehavior)]
    public string GeneratorVersion => Document.Generator;
#else
    private readonly RichTextBoxTextHost _host;

    public TextHost Host => _host;
    protected virtual RichTextBoxTextHost CreateTextHost(TextServicesGenerator generator, RichTextBox richTextBox) => new(generator, richTextBox);

    [System.Runtime.InteropServices.Marshalling.GeneratedComClass]
    protected partial class RichTextBoxTextHost(TextServicesGenerator generator, RichTextBox richTextBox) : TextHost(generator)
    {
        public RichTextBox RichTextBox { get; } = richTextBox;

        // avoid too many traces in debug
        public override uint TxGetSysColor(SYS_COLOR_INDEX nIndex) => Functions.GetSysColor(nIndex);
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

        public unsafe override HRESULT TxGetWindow(nint phwnd)
        {
            if (phwnd == 0 || RichTextBox == null)
                return base.TxGetWindow(phwnd);

            var window = RichTextBox.Window;
            if (window == null)
                return base.TxGetWindow(phwnd);

            *(HWND*)phwnd = window.Handle;
            return WiceCommons.S_OK;
        }

        public override void TxViewChange(BOOL fUpdate)
        {
            RichTextBox?.Invalidate(VisualPropertyInvalidateModes.Render);
        }

        public override unsafe void TxInvalidateRect(nint prc, BOOL fMode)
        {
            RichTextBox?.Invalidate(VisualPropertyInvalidateModes.Render);
        }
    }

    [Category(CategoryLive)]
    public IComObject<ITextDocument2>? Document => _host?.Document;

    [Category(CategoryBehavior)]
    public string GeneratorVersion => _host?.Generator ?? string.Empty;

#endif

    [Category(CategoryBehavior)]
    public TextServicesGenerator Generator { get; }

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

    // only works with Office generator
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

    public HRESULT SendMessage(uint msg, LPARAM lParam) => SendMessage(msg, 0, lParam);
    public HRESULT SendMessage(uint msg, WPARAM wParam) => SendMessage(msg, wParam, 0);
    public HRESULT SendMessage(uint msg, WPARAM wParam, LPARAM lParam) => SendMessage(msg, wParam, lParam, out _);
    public HRESULT SendMessage(uint msg, WPARAM wParam, LPARAM lParam, out LRESULT result)
    {
        result = default;
        if (_host == null)
            return Constants.E_FAIL;

        return _host.SendMessage(msg, wParam, lParam, out result);
    }

    public virtual string? GetText(tomConstants flags) => _host?.GetText(flags);
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

    protected virtual void Invalidate(VisualPropertyInvalidateModes modes = VisualPropertyInvalidateModes.Measure, [CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(propertyName);
        CheckRunningAsMainThread();
        base.Invalidate(modes);
    }

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

        var size = host.GetNaturalSize(NaturalSize, constraint).ToD2D_SIZE_F();
        D2D_SIZE_U dpi;
        if (Window != null && Window.Handle != 0)
        {
            dpi = DpiUtilities.GetDpiForWindow(Window.Handle);
        }
        else
        {
            dpi = DpiUtilities.GetDpiForDesktop();
        }

        if (dpi.width != WiceCommons.USER_DEFAULT_SCREEN_DPI)
        {
            size.width = size.width * WiceCommons.USER_DEFAULT_SCREEN_DPI / dpi.width;
        }

        if (dpi.height != WiceCommons.USER_DEFAULT_SCREEN_DPI)
        {
            size.height = size.height * WiceCommons.USER_DEFAULT_SCREEN_DPI / dpi.height;
        }

        var ratio = GetMonitorDpiRatioToPrimary(Window?.Monitor);
        size.width = size.width * ratio.Monitor / ratio.Primary;
        size.height = size.height * ratio.Monitor / ratio.Primary;

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

    protected override void ArrangeCore(D2D_RECT_F finalRect)
    {
        base.ArrangeCore(finalRect);
        var host = _host;
        if (host == null)
            return;

        var rc = GetRect(finalRect);
        host.Activate(rc);
    }

    protected internal override void RenderCore(RenderContext context)
    {
        base.RenderCore(context);
        var host = _host;
        if (host == null)
            return;

        var rc = GetRect(ArrangedRect);
        D2D_SIZE_U dpi;
        if (Window?.Handle.Value != 0)
        {
            dpi = DpiUtilities.GetDpiForWindow(Window!.Handle);
        }
        else
        {
            dpi = DpiUtilities.GetDpiForDesktop();
        }

        if (dpi.width != WiceCommons.USER_DEFAULT_SCREEN_DPI)
        {
            rc.Width = (int)(rc.Width * dpi.width / WiceCommons.USER_DEFAULT_SCREEN_DPI);
        }

        if (dpi.height != WiceCommons.USER_DEFAULT_SCREEN_DPI)
        {
            rc.Height = (int)(rc.Height * dpi.height / WiceCommons.USER_DEFAULT_SCREEN_DPI);
        }

        if (dpi.width != WiceCommons.USER_DEFAULT_SCREEN_DPI)
        {
            rc.Width = (int)(rc.Width * dpi.width * dpi.width / WiceCommons.USER_DEFAULT_SCREEN_DPI / WiceCommons.USER_DEFAULT_SCREEN_DPI);
        }

        if (dpi.height != WiceCommons.USER_DEFAULT_SCREEN_DPI)
        {
            rc.Height = (int)(rc.Height * dpi.height * dpi.height / WiceCommons.USER_DEFAULT_SCREEN_DPI / WiceCommons.USER_DEFAULT_SCREEN_DPI);
        }

        var ratio = GetMonitorDpiRatioToPrimary(Window.Monitor);
        rc.Width = (int)((long)rc.Width * ratio.Primary * ratio.Primary / ratio.Monitor / ratio.Monitor);
        rc.Height = (int)((long)rc.Height * ratio.Primary * ratio.Primary / ratio.Monitor / ratio.Monitor);

        context.DeviceContext.Object.SetUnitMode(D2D1_UNIT_MODE.D2D1_UNIT_MODE_PIXELS);
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

#if NETFRAMEWORK
        // TODO when DirectN nuget is updated, uncomment these lines and remove the one above
        //var rr = RelativeRenderRect;
        //var urc = rc;
        //urc.top = -(int)rr.top;
        //host.Draw(context.DeviceContext.Object, rc, urc);

        _host.Draw(context.DeviceContext.Object, rc);
#else
        host.Draw(context.DeviceContext.Object, rc, urc);
#endif
    }

    private IViewerParent? GetViewerParent() => Parent is Viewer viewer ? viewer.Parent as ScrollViewer : null;

    // seems like richedit is relative to primary monitor's dpi
    private static (int Primary, int Monitor) GetMonitorDpiRatioToPrimary(Monitor? monitor)
    {
        if (monitor == null || monitor.IsPrimary || monitor.EffectiveDpi.width == 0)
            return (1, 1);

        var primary = Monitor.Primary;
        if (primary == null || primary.EffectiveDpi.width == 0)
            return (1, 1);

        return ((int)primary.EffectiveDpi.width, (int)monitor.EffectiveDpi.width);
    }

    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == BackgroundColorProperty)
        {
            var host = _host;
            if (host != null)
            {
                host.BackColor = TextHost.ToColor((D3DCOLORVALUE)value!);
            }
            return true;
        }

        return true;
    }

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

    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }

    // allow command line change
#if NETFRAMEWORK
    public static TextServicesGenerator GetDefaultTextServicesGenerator() => CommandLine.GetArgument(nameof(TextServicesGenerator), TextServicesGenerator.Default);
#else
    public static TextServicesGenerator GetDefaultTextServicesGenerator() => CommandLine.Current.GetArgument(nameof(TextServicesGenerator), TextServicesGenerator.Default);
#endif

    private static readonly Lazy<string> _defaultTextServicesGeneratorVersion = new(GetDefaultTextServicesGeneratorVersion);
    public static string GetDefaultTextServicesGeneratorVersion()
    {
        using var rtb = new RichTextBox();
        return rtb.GeneratorVersion;
    }

    public static string DefaultTextServicesGeneratorVersion { get; } = _defaultTextServicesGeneratorVersion.Value;
}
