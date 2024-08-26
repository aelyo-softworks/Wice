namespace Wice;

// this currently is only read-only
// note this visual sits on a COM object so we must dispose it on the same thread that created it
public partial class RichTextBox : RenderVisual, IDisposable
{
    private TextHost? _host;
    private bool _disposedValue;

    public RichTextBox(TextServicesGenerator generator = TextServicesGenerator.Default)
    {
        Generator = generator;
        _host = new TextHost(generator)
        {
            TextColor = new COLORREF()
        };
        BackgroundColor = D3DCOLORVALUE.Transparent;
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
        get => (_host?.Options).GetValueOrDefault();
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
        Application.CheckRunningAsMainThread();
        Invalidate(modes, new InvalidateReason(GetType()));
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

        context.DeviceContext.Object.SetUnitMode(D2D1_UNIT_MODE.D2D1_UNIT_MODE_PIXELS);
        host.Draw(context.DeviceContext.Object, rc);
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
}
