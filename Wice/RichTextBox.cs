using System;
using System.ComponentModel;
using System.Threading;
using DirectN;
using Wice.Utilities;

namespace Wice
{
    // this currently is only read-only
    // note this visual sits on a COM object so we must dispose it on the same thread that created it
    public class RichTextBox : RenderVisual, IDisposable
    {
        private TextHost _host;
        private bool _disposedValue;

        public RichTextBox(TextServicesGenerator generator = TextServicesGenerator.Default)
        {
            if (generator == TextServicesGenerator.Default)
            {
                generator = GetDefaultTextServicesGenerator();
            }

            Generator = generator;
            _host = new TextHost(generator);
            _host.TextColor = 0;
            BackgroundColor = _D3DCOLORVALUE.Transparent;
        }

        protected TextHost Host => _host;

        [Category(CategoryLive)]
        public dynamic Document => _host?.Document;

        [Category(CategoryBehavior)]
        public TextServicesGenerator Generator { get; }

        [Category(CategoryBehavior)]
        public string GeneratorVersion => Document.Generator;

        [Category(CategoryRender)]
        public virtual _D3DCOLORVALUE TextColor
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

        [Category(CategoryBehavior)]
        public virtual string RtfText
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
        [Category(CategoryBehavior)]
        public virtual string HtmlText
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
            D2D_SIZE_U dpi;
            if (Window?.Handle != IntPtr.Zero)
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

            var ratio = GetMonitorDpiRatioToPrimary(Window.Monitor);
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

        private tagRECT GetRect(D2D_RECT_F finalRect)
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
            return rc.TotagRECT();
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
            if (Window?.Handle != IntPtr.Zero)
            {
                dpi = DpiUtilities.GetDpiForWindow(Window.Handle);
            }
            else
            {
                dpi = DpiUtilities.GetDpiForDesktop();
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

            context.DeviceContext.Object.SetUnitMode(D2D1_UNIT_MODE.D2D1_UNIT_MODE_DIPS);
            _host.Draw(context.DeviceContext.Object, rc);

            // TODO when DirectN nuget is updated, uncomment these lines and remove the one above
            //var rr = RelativeRenderRect;
            //var urc = rc;
            //urc.top = -(int)rr.top;
            //host.Draw(context.DeviceContext.Object, rc, urc);
        }

        // seems like richedit is relative to primary monitor's dpi
        private static (int Primary, int Monitor) GetMonitorDpiRatioToPrimary(DirectN.Monitor monitor)
        {
            if (monitor == null || monitor.IsPrimary || monitor.EffectiveDpi.width == 0)
                return (1, 1);

            var primary = DirectN.Monitor.Primary;
            if (primary == null || primary.EffectiveDpi.width == 0)
                return (1, 1);

            return ((int)primary.EffectiveDpi.width, (int)monitor.EffectiveDpi.width);
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
        public static TextServicesGenerator GetDefaultTextServicesGenerator() => CommandLine.GetArgument(nameof(TextServicesGenerator), TextServicesGenerator.Default);

        public static string GetDefaultTextServicesGeneratorVersion()
        {
            using (var rtb = new RichTextBox())
            {
                return rtb.GeneratorVersion;
            }
        }
    }
}
