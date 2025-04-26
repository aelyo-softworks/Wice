using System.Runtime.InteropServices.Marshalling;

namespace Wice;

// this currently is only read-only
// note this visual sits on a COM object so we must dispose it on the same thread that created it
public partial class RichTextBox : RenderVisual, IDisposable
{
    private RichTextBoxTextHost? _host;
    private bool _disposedValue;

    public RichTextBox(TextServicesGenerator generator = TextServicesGenerator.Default)
    {
        if (generator == TextServicesGenerator.Default)
        {
            generator = GetDefaultTextServicesGenerator();
        }

        Generator = generator;
        _host = CreateTextHost(generator, this);
        _host.TextColor = new COLORREF();
        BackgroundColor = D3DCOLORVALUE.Transparent;
    }

    protected RichTextBoxTextHost? Host => _host;
    protected virtual RichTextBoxTextHost CreateTextHost(TextServicesGenerator generator, RichTextBox richTextBox) => new(generator, richTextBox);

    [GeneratedComClass]
    protected partial class RichTextBoxTextHost(TextServicesGenerator generator, RichTextBox richTextBox)
        : TextHost(generator)
    {
        public RichTextBox RichTextBox { get; } = richTextBox;

        // avoid too many traces in debug
        public override uint TxGetSysColor(SYS_COLOR_INDEX nIndex) => Functions.GetSysColor(nIndex);
        public unsafe override bool TxClientToScreen(nint lppt)
        {
            if (lppt == 0)
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
            if (lppt == 0)
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

        public unsafe override HRESULT TxGetPropertyBits(uint dwMask, nint pdwBits)
        {
            var mask = (TXTBIT)dwMask;
            if (pdwBits == 0)
                return Constants.E_INVALIDARG;

            var bits = TXTBIT.TXTBIT_RICHTEXT | TXTBIT.TXTBIT_D2DDWRITE;
            if (Options.HasFlag(TextHostOptions.WordWrap))
            {
                bits |= TXTBIT.TXTBIT_WORDWRAP;
            }

            if (Options.HasFlag(TextHostOptions.Vertical))
            {
                bits |= TXTBIT.TXTBIT_VERTICAL;
            }

            if (Options.HasFlag(TextHostOptions.ReadOnly))
            {
                bits |= TXTBIT.TXTBIT_READONLY;
            }

            if (Options.HasFlag(TextHostOptions.Multiline))
            {
                bits |= TXTBIT.TXTBIT_MULTILINE;
            }

            bits &= mask;
            *(TXTBIT*)pdwBits = bits;
            return Constants.S_OK;
        }

        public unsafe override HRESULT TxGetSelectionBarWidth(nint lSelBarWidth)
        {
            if (lSelBarWidth == 0)
                return Constants.E_INVALIDARG;

            *(int*)lSelBarWidth = 0;
            return Constants.S_OK;
        }

        public unsafe override HRESULT TxGetWindowStyles(nint pdwStyle, nint pdwExStyle)
        {
            var window = RichTextBox.Window;
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
            return Constants.S_OK;
        }

        public unsafe override HRESULT TxGetWindow(nint phwnd)
        {
            if (phwnd == 0)
                return base.TxGetWindow(phwnd);

            var window = RichTextBox.Window;
            if (window == null)
                return base.TxGetWindow(phwnd);

            *(HWND*)phwnd = window.Handle;
            return Constants.S_OK;
        }

        public override void TxViewChange(BOOL fUpdate)
        {
            RichTextBox.Invalidate(VisualPropertyInvalidateModes.Render);
        }

        public override unsafe void TxInvalidateRect(nint prc, BOOL fMode)
        {
            RichTextBox.Invalidate(VisualPropertyInvalidateModes.Render);
        }
    }

    [Category(CategoryLive)]
    public IComObject<ITextDocument2>? Document => _host?.Document;

    [Category(CategoryBehavior)]
    public TextServicesGenerator Generator { get; }

    [Category(CategoryBehavior)]
    public string GeneratorVersion => _host?.Generator ?? string.Empty;

    [Category(CategoryRender)]
    public virtual D3DCOLORVALUE TextColor
    {
        get => TextHost.ToColor((_host?.TextColor).GetValueOrDefault());
        set
        {
            var host = _host;
            if (host == null)
                return;

            OnPropertyChanging(nameof(Options));
            host.TextColor = TextHost.ToColor(value);
            Invalidate(nameof(Options), VisualPropertyInvalidateModes.Render);
        }
    }

    [Category(CategoryLayout)]
    public virtual TextHostOptions Options
    {
        get => _host?.Options ?? TextHostOptions.Default;
        set
        {
            var host = _host;
            if (host == null)
                return;

            OnPropertyChanging(nameof(Options));
            host.Options = value;
            Invalidate(nameof(Options));
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

            OnPropertyChanging(nameof(Text));
            host.Text = value;
            Invalidate(nameof(Text));
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

            OnPropertyChanging(nameof(RtfText));
            host.RtfText = value;
            Invalidate(nameof(RtfText));
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

            OnPropertyChanging(nameof(HtmlText));
            host.HtmlText = value;
            Invalidate(nameof(HtmlText));
        }
    }

    protected virtual void Invalidate(string propertyName, VisualPropertyInvalidateModes modes = VisualPropertyInvalidateModes.Measure)
    {
        OnPropertyChanging(propertyName);
        CheckRunningAsMainThread();
        Invalidate(modes);
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

        var size = host.GetNaturalSize(TXTNATURALSIZE.TXTNS_FITTOCONTENT, constraint).ToD2D_SIZE_F();
        D2D_SIZE_U dpi;
        if (Window != null && Window.Handle != 0)
        {
            dpi = DpiUtilities.GetDpiForWindow(Window.Handle);
        }
        else
        {
            dpi = DpiUtilities.GetDpiForDesktop();
        }

        if (dpi.width != 96)
        {
            size.width = size.width * 96 / dpi.width;
        }

        if (dpi.height != 96)
        {
            size.height = size.height * 96 / dpi.height;
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

        if (dpi.width != 96)
        {
            rc.Width = (int)(rc.Width * dpi.width / 96);
        }

        if (dpi.height != 96)
        {
            rc.Height = (int)(rc.Height * dpi.height / 96);
        }

        if (dpi.width != 96)
        {
            rc.Width = (int)(rc.Width * dpi.width * dpi.width / 96 / 96);
        }

        if (dpi.height != 96)
        {
            rc.Height = (int)(rc.Height * dpi.height * dpi.height / 96 / 96);
        }

        var ratio = GetMonitorDpiRatioToPrimary(Window.Monitor);
        rc.Width = rc.Width * ratio.Primary * ratio.Primary / ratio.Monitor / ratio.Monitor;
        rc.Height = rc.Height * ratio.Primary * ratio.Primary / ratio.Monitor / ratio.Monitor;

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

        host.Draw(context.DeviceContext.Object, rc, urc);
    }

    private IViewerParent? GetViewerParent() => Parent is Viewer viewer ? viewer.Parent as ScrollViewer : null;

    // seems like richedit is relative to primary monitor's dpi
    private static (int Primary, int Monitor) GetMonitorDpiRatioToPrimary(DirectN.Extensions.Utilities.Monitor? monitor)
    {
        if (monitor == null || monitor.IsPrimary || monitor.EffectiveDpi.width == 0)
            return (1, 1);

        var primary = DirectN.Extensions.Utilities.Monitor.Primary;
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
                    Interlocked.Exchange(ref _host, null)?.Dispose();
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

    ~RichTextBox() { Dispose(disposing: false); }
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }

    // allow command line change
    public static TextServicesGenerator GetDefaultTextServicesGenerator() => CommandLine.Current.GetArgument(nameof(TextServicesGenerator), TextServicesGenerator.Default);

    private static readonly Lazy<string> _defaultTextServicesGeneratorVersion = new(GetDefaultTextServicesGeneratorVersion);
    private static string GetDefaultTextServicesGeneratorVersion()
    {
        using var rtb = new RichTextBox();
        return rtb.GeneratorVersion;
    }

    public static string DefaultTextServicesGeneratorVersion { get; } = _defaultTextServicesGeneratorVersion.Value;
}
