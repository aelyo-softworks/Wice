using System;
using System.Threading;
using DirectN;

namespace Wice
{
    // this currently is only read-only
    public class RichTextBox : RenderVisual, IDisposable
    {
        private TextHost _host;
        private bool _disposedValue;

#if DEBUG
        public static ILogger Logger { get => TextHost.Logger; set => TextHost.Logger = value; }
#endif

        public RichTextBox(TextServicesGenerator generator = TextServicesGenerator.Default)
        {
            Generator = generator;
            _host = new TextHost(generator);
            _host.TextColor = 0;
            BackgroundColor = _D3DCOLORVALUE.Transparent;
        }

        public dynamic Document => _host?.Document;
        public TextServicesGenerator Generator { get; }
        public string GeneratorVersion => Document.Generator;

        public _D3DCOLORVALUE TextColor
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

        public TextHostOptions Options
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

        public string Text
        {
            get => _host?.Text;
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

        public string RtfText
        {
            get => _host?.RtfText;
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
        public string HtmlText
        {
            get => _host?.HtmlText;
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

        private void Invalidate(string propertyName, VisualPropertyInvalidateModes modes = VisualPropertyInvalidateModes.Measure)
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

            var size = host.GetNaturalSize(TXTNATURALSIZE.TXTNS_FITTOCONTENT, constraint);
            return size.ToD2D_SIZE_F();
        }

        protected override void ArrangeCore(D2D_RECT_F finalRect)
        {
            base.ArrangeCore(finalRect);
            var rc = new tagRECT(0, 0, finalRect.Width, finalRect.Height);
            _host?.Activate(rc);
        }

        protected internal override void RenderCore(RenderContext context)
        {
            base.RenderCore(context);
            var ar = ArrangedRect;
            var rc = new tagRECT(0, 0, ar.Width, ar.Height);
            _host?.Draw(context.DeviceContext.Object, rc);
        }

        protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
        {
            if (!base.SetPropertyValue(property, value, options))
                return false;

            if (property == BackgroundColorProperty)
            {
                var host = _host;
                if (host != null)
                {
                    host.BackColor = TextHost.ToColor((_D3DCOLORVALUE)value);
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
                    Interlocked.Exchange(ref _host, null)?.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                _disposedValue = true;
            }
        }

        ~RichTextBox() { Dispose(disposing: false); }
        public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
    }
}
