using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DirectN;
using Wice.Utilities;

namespace Wice
{
    public class ResourceManager
    {
        private readonly ConcurrentDictionary<string, Resource> _resources = new ConcurrentDictionary<string, Resource>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<Window, WindowResources> _windowsResources = new ConcurrentDictionary<Window, WindowResources>();
        private Theme _theme;

        public ResourceManager(Application application)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            Application = application;
            D2DFactory = D2D1Functions.D2D1CreateFactory1();
            DWriteFactory = DWriteFunctions.DWriteCreateFactory();
            Theme = CreateTheme();
            if (Theme == null)
                throw new InvalidOperationException();
        }

        public Application Application { get; }
        public IComObject<ID2D1Factory1> D2DFactory { get; }
        public IComObject<IDWriteFactory> DWriteFactory { get; }
        public Theme Theme
        {
            get => _theme;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (_theme == value)
                    return;

                _theme = value;
            }
        }

        protected virtual Theme CreateTheme() => new Theme();

#if DEBUG
        internal void TraceInformation()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Resources: " + _resources.Count);
            foreach (var kv in _resources.OrderBy(k => k.Value.LastAccess))
            {
                sb.AppendLine(" '" + kv.Key.Replace('\0', '|') + "' holds " + kv.Value);
            }

            sb.AppendLine("Windows Resources: " + _windowsResources.Count);
            foreach (var kv in _windowsResources)
            {
                sb.AppendLine(" Window '" + kv.Key.Title + "' (Hwnd: 0x" + kv.Key.Native.Handle.ToString("X8") + ")");
                sb.AppendLine(" Resources: " + kv.Value._resources.Count);
                foreach (var kv2 in kv.Value._resources)
                {
                    sb.AppendLine("  " + kv2.Key + " holds " + kv2.Value);
                }

                sb.AppendLine(" Render Disposables: " + kv.Value._renderDisposables.Count);
                foreach (var rd in kv.Value._renderDisposables)
                {
                    sb.AppendLine("  " + rd);
                }
            }

            Application.Trace(sb.ToString());
        }
#endif

        private enum Domain
        {
            Undefined,
            StrokeStyle,
            WICBitmapSource,
            ScrollBarButtonGeometry,
            TextLayout,
            TextFormat,
            TitleBarButtonTypeGeometry,
            Typography,
            CheckButtonGeometry,
            ToggleSwitchGeometry,
        }

        private string GetKey(Domain domain, string name)
        {
            if (domain == Domain.Undefined)
                throw new ArgumentException(null, nameof(name));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return name + "\0" + (int)domain;
        }

        private T Get<T>(Window window, Domain domain, string name, T defaultValue = default, bool propertiesUseConversions = false)
        {
            var resources = window != null ? _windowsResources[window]._resources : _resources;
            if (!resources.TryGetValue(GetKey(domain, name), out var resource))
                return defaultValue;

            resource.LastAccess = DateTime.Now;
            if (propertiesUseConversions)
                return Conversions.ChangeType(resource.Object, defaultValue);

            return (T)resource.Object;
        }

        private T Get<T>(Window window, Domain domain, string name, Func<T> factory = null, bool propertiesUseConversions = false)
        {
            if (factory == null)
                return Get(window, domain, name, default(T));

            var resources = window != null ? _windowsResources[window]._resources : _resources;
            var key = GetKey(domain, name);
            if (!resources.TryGetValue(key, out var resource))
            {
                resource = new Resource();
                resource.Object = factory();
                return (T)resources.AddOrUpdate(key, resource, (k, o) =>
                {
                    resource.Dispose();
                    o.LastAccess = DateTime.Now;
                    return o;
                }).Object;
            }

            resource.LastAccess = DateTime.Now;
            if (propertiesUseConversions)
                return Conversions.ChangeType(resource.Object, default(T));

            return (T)resource.Object;
        }

        public virtual IComObject<IDWriteTypography> GetTypography(Typography typography)
        {
            if (typography == null)
                throw new ArgumentNullException(nameof(typography));

            var key = typography.CacheKey;
            return Get(null, Domain.Typography, key, () => CreateTypography(typography));
        }

        public virtual IComObject<IDWriteTypography> CreateTypography(Typography typography)
        {
            if (typography == null)
                throw new ArgumentNullException(nameof(typography));

            var tg = DWriteFactory.CreateTypography();
            foreach (var feature in typography.Features)
            {
                tg.Object.AddFontFeature(feature).ThrowOnError();
            }
            return tg;
        }

        public virtual IComObject<ID2D1StrokeStyle> GetStrokeStyle(D2D1_STROKE_STYLE_PROPERTIES value, float[] dashes = null)
        {
            var key = (int)value.dashCap + "\0" +
                (int)value.dashOffset + "\0" +
                (int)value.dashStyle + "\0" +
                (int)value.endCap + "\0" +
                (int)value.lineJoin + "\0" +
                (int)value.miterLimit + "\0" +
                (int)value.startCap;

            if (!dashes.IsEmpty())
            {
                key = key + "\0" + string.Join("\0", dashes.Select(d => d.ToString(CultureInfo.InvariantCulture)));
            }

            return Get(null, Domain.StrokeStyle, key, () =>
            {
                var strokeProps = new D2D1_STROKE_STYLE_PROPERTIES();
                strokeProps.startCap = D2D1_CAP_STYLE.D2D1_CAP_STYLE_FLAT;
                strokeProps.endCap = D2D1_CAP_STYLE.D2D1_CAP_STYLE_FLAT;
                strokeProps.dashCap = D2D1_CAP_STYLE.D2D1_CAP_STYLE_TRIANGLE;
                strokeProps.lineJoin = D2D1_LINE_JOIN.D2D1_LINE_JOIN_MITER;
                strokeProps.miterLimit = 10;
                strokeProps.dashStyle = D2D1_DASH_STYLE.D2D1_DASH_STYLE_DOT;
                strokeProps.dashOffset = 0;

                D2DFactory.Object.CreateStrokeStyle(ref strokeProps, dashes, (dashes?.Length).GetValueOrDefault(), out var stroke).ThrowOnError();
                return new ComObject<ID2D1StrokeStyle>(stroke);
            });
        }

        public virtual IComObject<IWICBitmapSource> GetWicBitmapSource(string filePath, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            filePath = System.IO.Path.GetFullPath(filePath);
            var key = filePath.ToLowerInvariant();
            return Get(null, Domain.WICBitmapSource, key, () => WICFunctions.LoadBitmapSource(filePath, options));
        }

        public virtual IComObject<IWICBitmapSource> GetWicBitmapSource(Assembly assembly, string name, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var stream = assembly.GetManifestResourceStream(name);
            if (stream == null)
                throw new WiceException("0016: cannot find stream '" + name + "' from assembly '" + assembly.FullName + "'.");

            var key = assembly.FullName + "\0" + name;
            return GetWicBitmapSource(stream, key, options);
        }

        // note: keep the stream open as wic lazy loads
        public virtual IComObject<IWICBitmapSource> GetWicBitmapSource(Stream stream, string uniqueKey, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (uniqueKey == null)
                throw new ArgumentNullException(nameof(uniqueKey));

            return Get(null, Domain.WICBitmapSource, uniqueKey, () => WICFunctions.LoadBitmapSource(stream, options));
        }

        public virtual IComObject<IDWriteTextFormat> GetSymbolFormat(float fontSize = 0) => GetTextFormat(Theme.SymbolFontName, fontSize);
        public virtual IComObject<IDWriteTextFormat> GetTextFormat(
            string fontFamilyName = null,
            float fontSize = 0,
            DWRITE_PARAGRAPH_ALIGNMENT paragraphAlignment = DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER,
            DWRITE_TEXT_ALIGNMENT textAlignment = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER,
            IComObject<IDWriteFontCollection> fontCollection = null
            )
        {
            var text = new TextFormat();
            text.FontFamilyName = fontFamilyName;
            text.FontSize = fontSize;
            text.ParagraphAlignment = paragraphAlignment;
            text.Alignment = textAlignment;
            text.FontCollection = fontCollection;
            return GetTextFormat(text);
        }

        public virtual IComObject<IDWriteTextFormat> GetTextFormat(ITextFormat text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var family = text.FontFamilyName.Nullify() ?? Theme.DefaultFontFamilyName;
            var size = GetFontSize(text);
            var key = TextFormat.GetCacheKey(text, family, size);
            return Get(null, Domain.TextFormat, key, () => CreateTextFormat(text));
        }

        private float GetFontSize(ITextFormat text) => GetFontSize(text?.FontSize);
        internal float GetFontSize(float? size)
        {
            if (!size.HasValue)
                return Theme.DefaultFontSize;

            if (size.Value <= 0)
                return Theme.DefaultFontSize;

            return size.Value;
        }

        public virtual IComObject<IDWriteTextFormat> CreateTextFormat(ITextFormat text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var family = text.FontFamilyName.Nullify() ?? Theme.DefaultFontFamilyName;
            IComObject<IDWriteTextFormat> format;
            IComObject<IDWriteInlineObject> io = null;
            var size = GetFontSize(text);
            format = CreateTextFormat(family, size, text.FontCollection?.Object, text.FontWeight, text.FontStyle, text.FontStretch);
            if (text.TrimmingGranularity == DWRITE_TRIMMING_GRANULARITY.DWRITE_TRIMMING_GRANULARITY_CHARACTER)
            {
                io = DWriteFactory.CreateEllipsisTrimmingSign(format);
            }

            try
            {
                format.Object.SetWordWrapping(text.WordWrapping).ThrowOnError();
                format.Object.SetParagraphAlignment(text.ParagraphAlignment).ThrowOnError();
                format.Object.SetTextAlignment(text.Alignment).ThrowOnError();
                format.Object.SetFlowDirection(text.FlowDirection).ThrowOnError();
                format.Object.SetReadingDirection(text.ReadingDirection).ThrowOnError();

                var to = new DWRITE_TRIMMING();
                to.granularity = text.TrimmingGranularity;
                format.Object.SetTrimming(ref to, io?.Object);
                return format;
            }
            finally
            {
                io?.Dispose();
            }
        }

        public virtual IComObject<IDWriteTextFormat> GetTextFormat(
            string fontFamilyName = null,
            float? fontSize = null,
            IDWriteFontCollection fonts = null,
            DWRITE_FONT_WEIGHT fontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_NORMAL,
            DWRITE_FONT_STYLE fontStyle = DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_NORMAL,
            DWRITE_FONT_STRETCH fontStretch = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_NORMAL)
        {
            var family = fontFamilyName.Nullify() ?? Theme.DefaultFontFamilyName;
            var size = GetFontSize(fontSize);
            var key = family + "\0" + size + "\0" + TextFormat.GetCacheKey(fonts) + "\0" + ((int)fontWeight) + "\0" + ((int)fontStyle) + "\0" + ((int)fontStretch);
            return Get(null, Domain.TextFormat, key, () => CreateTextFormat(fontFamilyName, fontSize, fonts, fontWeight, fontStyle, fontStretch));
        }

        public virtual IComObject<IDWriteTextFormat> CreateTextFormat(
            string fontFamilyName = null,
            float? fontSize = null,
            IDWriteFontCollection fonts = null,
            DWRITE_FONT_WEIGHT fontWeight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_NORMAL,
            DWRITE_FONT_STYLE fontStyle = DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_NORMAL,
            DWRITE_FONT_STRETCH fontStretch = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_NORMAL)
        {
            var family = fontFamilyName.Nullify() ?? Theme.DefaultFontFamilyName;
            var size = GetFontSize(fontSize);
            var key = family + "\0" + size + "\0" + TextFormat.GetCacheKey(fonts);
            DWriteFactory.Object.CreateTextFormat(family, fonts, fontWeight, fontStyle, fontStretch, size, string.Empty, out var format).ThrowOnError();
            return new KeyComObject<IDWriteTextFormat>(format, key);
        }

        public IComObject<IDWriteTextLayout> GetTextLayout(IComObject<IDWriteTextFormat> format, string text, int textLength = 0, float maxWidth = float.MaxValue, float maxHeight = float.MaxValue) => GetTextLayout(format?.Object, text, textLength, maxWidth, maxHeight);
        public virtual IComObject<IDWriteTextLayout> GetTextLayout(IDWriteTextFormat format, string text, int textLength = 0, float maxWidth = float.MaxValue, float maxHeight = float.MaxValue)
        {
            if (text == null)
                return null;

            if (format == null)
                throw new ArgumentNullException(nameof(format));

            if (!(format is IKeyable kf))
                return CreateTextLayout<IDWriteTextLayout>(format, text, textLength, maxWidth, maxHeight);

            var key = textLength + "\0" + maxWidth + "\0" + maxHeight + "\0" + kf.Key + "\0" + text;
            return Get(null, Domain.TextLayout, key, () =>
            {
                return CreateTextLayout<IDWriteTextLayout>(format, text, textLength, maxWidth, maxHeight);
            });
        }

        public virtual IComObject<IDWriteTextLayout> CreateTextLayout(IComObject<IDWriteTextFormat> format, string text, int textLength = 0, float maxWidth = float.MaxValue, float maxHeight = float.MaxValue)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (format == null)
                throw new ArgumentNullException(nameof(format));

            if (maxWidth.IsNotSet())
            {
                maxWidth = float.MaxValue;
            }

            if (maxHeight.IsNotSet())
            {
                maxHeight = float.MaxValue;
            }

            return CreateTextLayout<IDWriteTextLayout>(format.Object, text, textLength, maxWidth, maxHeight);
        }

        private IComObject<T> CreateTextLayout<T>(
            IDWriteTextFormat format,
            string text,
            int textLength = 0,
            float maxWidth = float.MaxValue,
            float maxHeight = float.MaxValue
            ) where T : IDWriteTextLayout
        {
            textLength = textLength <= 0 ? text.Length : textLength;
            DWriteFactory.Object.CreateTextLayout(text, (uint)textLength, format, maxWidth, maxHeight, out var layout).ThrowOnError();
            return new ComObject<T>((T)layout);
        }

        public GeometrySource2D GetToggleSwitchGeometrySource(float width, float height, float margin) => new GeometrySource2D("ToggleSwitch" + width + "\0" + height + "\0" + margin) { Geometry = Get(null, Domain.ToggleSwitchGeometry, width + "\0" + height + "\0" + margin, () => CreateToggleSwitchGeometry(width, height, margin)).Object };

        //  margin is used as offset, so stroke can be fully seen
        public virtual IComObject<ID2D1PathGeometry1> GetToggleSwitchGeometry(float width, float height, float margin) => Get(null, Domain.ToggleSwitchGeometry, width + "\0" + height + "\0" + margin, () => CreateToggleSwitchGeometry(width, height, margin));
        public virtual IComObject<ID2D1PathGeometry1> CreateToggleSwitchGeometry(float width, float height, float margin)
        {
            var path = D2DFactory.CreatePathGeometry();
            using (var sink = path.Open<ID2D1GeometrySink>())
            {
                var size = new D2D_SIZE_F(width / 4, height / 2);
                sink.BeginFigure(new D2D_POINT_2F(margin + width / 4, margin));
                sink.AddArc(new D2D_POINT_2F(margin + width / 4, margin + height), size);
                sink.AddLine(new D2D_POINT_2F(margin + width * 3 / 4, margin + height));
                sink.AddArc(new D2D_POINT_2F(margin + width * 3 / 4, margin), size);
                sink.EndFigure(D2D1_FIGURE_END.D2D1_FIGURE_END_CLOSED);
                sink.Close();
            }
            return path;
        }

        public GeometrySource2D GetCheckButtonGeometrySource(float width, float height) => new GeometrySource2D("CheckButton" + width + "\0" + height) { Geometry = Get(null, Domain.CheckButtonGeometry, width + "\0" + height, () => CreateCheckButtonGeometry(width, height)).Object };

        public virtual IComObject<ID2D1PathGeometry1> GetCheckButtonGeometry(float width, float height) => Get(null, Domain.CheckButtonGeometry, width + "\0" + height, () => CreateCheckButtonGeometry(width, height));
        public virtual IComObject<ID2D1PathGeometry1> CreateCheckButtonGeometry(float width, float height)
        {
            var path = D2DFactory.CreatePathGeometry();
            using (var sink = path.Open())
            {
                sink.BeginFigure(new D2D_POINT_2F(width / 8, height / 2), D2D1_FIGURE_BEGIN.D2D1_FIGURE_BEGIN_HOLLOW);
                sink.AddLines(new D2D_POINT_2F(width / 3, height * 3 / 4), new D2D_POINT_2F(7 * width / 8, height / 4));
                sink.EndFigure();
                sink.Close();
            }
            return path;
        }

        public GeometrySource2D GetScrollBarButtonGeometrySource(DockType type, float width, float ratio, bool open) => new GeometrySource2D("ScrollBarButton" + ((int)type) + "\0" + width + "\0" + ratio + "\0" + (open ? 1 : 0)) { Geometry = Get(null, Domain.ScrollBarButtonGeometry, ((int)type) + "\0" + width + "\0" + ratio + "\0" + (open ? 1 : 0), () => CreateScrollBarButtonGeometry(type, width, ratio, open)).Object };

        public virtual IComObject<ID2D1PathGeometry1> GetScrollBarButtonGeometry(DockType type, float width, float ratio, bool open) => Get(null, Domain.ScrollBarButtonGeometry, ((int)type) + "\0" + width + "\0" + ratio + "\0" + (open ? 1 : 0), () => CreateScrollBarButtonGeometry(type, width, ratio, open));
        public virtual IComObject<ID2D1PathGeometry1> CreateScrollBarButtonGeometry(DockType type, float width, float ratio, bool open)
        {
            if (ratio.IsNotSet() || ratio <= 0)
            {
                ratio = open ? 0.5f : 0.7f;
            }

            var path = D2DFactory.CreatePathGeometry();
            using (var sink = path.Open())
            {
                D2D_POINT_2F start;
                D2D_POINT_2F arrow;
                D2D_POINT_2F end;
                switch (type)
                {
                    case DockType.Top:
                        start = new D2D_POINT_2F(0f, width * ratio);
                        arrow = new D2D_POINT_2F(width / 2, 0f);
                        end = new D2D_POINT_2F(width, width * ratio);
                        break;

                    case DockType.Bottom:
                        start = new D2D_POINT_2F(0f, width - width * ratio);
                        arrow = new D2D_POINT_2F(width / 2, width);
                        end = new D2D_POINT_2F(width, width - width * ratio);
                        break;

                    case DockType.Right:
                        start = new D2D_POINT_2F(width - width * ratio, 0f);
                        arrow = new D2D_POINT_2F(width, width / 2);
                        end = new D2D_POINT_2F(width - width * ratio, width);
                        break;

                    case DockType.Left:
                        start = new D2D_POINT_2F(width * ratio, 0f);
                        arrow = new D2D_POINT_2F(0f, width / 2);
                        end = new D2D_POINT_2F(width * ratio, width);
                        break;

                    default:
                        throw new NotSupportedException();
                }

                sink.BeginFigure(start, open ? D2D1_FIGURE_BEGIN.D2D1_FIGURE_BEGIN_HOLLOW : D2D1_FIGURE_BEGIN.D2D1_FIGURE_BEGIN_FILLED);
                sink.AddLines(arrow, end);
                sink.EndFigure(open ? D2D1_FIGURE_END.D2D1_FIGURE_END_OPEN : D2D1_FIGURE_END.D2D1_FIGURE_END_CLOSED);
                sink.Close();
            }
            return path;
        }

        public GeometrySource2D GetTitleBarButtonGeometrySource(TitleBarButtonType type, float width) => new GeometrySource2D("TitleBarButton" + ((int)type) + "\0" + width) { Geometry = GetTitleBarButtonGeometry(type, width).Object };

        public virtual IComObject<ID2D1PathGeometry1> GetTitleBarButtonGeometry(TitleBarButtonType type, float width) => Get(null, Domain.TitleBarButtonTypeGeometry, ((int)type) + "\0" + width, () => CreateTitleBarButtonGeometry(type, width));
        public virtual IComObject<ID2D1PathGeometry1> CreateTitleBarButtonGeometry(TitleBarButtonType type, float width)
        {
            var path = D2DFactory.CreatePathGeometry();
            using (var sink = path.Open())
            {
                switch (type)
                {
                    case TitleBarButtonType.Close:
                        sink.BeginFigure(new D2D_POINT_2F(), D2D1_FIGURE_BEGIN.D2D1_FIGURE_BEGIN_HOLLOW);
                        sink.AddLine(new D2D_POINT_2F(width, width));
                        sink.EndFigure(D2D1_FIGURE_END.D2D1_FIGURE_END_OPEN);
                        sink.BeginFigure(new D2D_POINT_2F(width, 0), D2D1_FIGURE_BEGIN.D2D1_FIGURE_BEGIN_HOLLOW);
                        sink.AddLine(new D2D_POINT_2F(0, width));
                        sink.EndFigure();
                        break;

                    case TitleBarButtonType.Minimize:
                        const float forceAliasing = 0.5f; // works only if width is integral...
                        sink.BeginFigure(new D2D_POINT_2F(0, width / 2 + forceAliasing), D2D1_FIGURE_BEGIN.D2D1_FIGURE_BEGIN_HOLLOW);
                        sink.AddLine(new D2D_POINT_2F(width, width / 2 + forceAliasing));
                        sink.EndFigure();
                        break;

                    case TitleBarButtonType.Restore:
                        //   +---------+
                        //   |         |
                        // +-+-------+ |
                        // |         | |
                        // |         | |
                        // |         | |
                        // |         +-+
                        // +---------+
                        const float offset = 2.5f;
                        sink.BeginFigure(new D2D_POINT_2F(offset, offset), D2D1_FIGURE_BEGIN.D2D1_FIGURE_BEGIN_HOLLOW);
                        sink.AddLines(new D2D_POINT_2F(offset, 0), new D2D_POINT_2F(width, 0), new D2D_POINT_2F(width, width - offset), new D2D_POINT_2F(width - offset, width - offset));
                        sink.EndFigure();

                        sink.BeginFigure(new D2D_POINT_2F(0, offset), D2D1_FIGURE_BEGIN.D2D1_FIGURE_BEGIN_HOLLOW);
                        sink.AddLines(new D2D_POINT_2F(width - offset, offset), new D2D_POINT_2F(width - offset, width), new D2D_POINT_2F(0, width));
                        sink.EndFigure(D2D1_FIGURE_END.D2D1_FIGURE_END_CLOSED);
                        break;

                    case TitleBarButtonType.Maximize:
                        sink.BeginFigure(new D2D_POINT_2F(0, 0), D2D1_FIGURE_BEGIN.D2D1_FIGURE_BEGIN_HOLLOW);
                        sink.AddLines(new D2D_POINT_2F(width, 0), new D2D_POINT_2F(width, width), new D2D_POINT_2F(0, width));
                        sink.EndFigure(D2D1_FIGURE_END.D2D1_FIGURE_END_CLOSED);
                        break;
                }
                sink.Close();
            }
            return path;
        }

        public virtual void AddRenderDisposable(Window window, IDisposable disposable)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            if (disposable == null)
                return;

            var win = window.Window;
            if (win == null)
                return;

            _windowsResources[win]._renderDisposables.Add(new RenderDisposable(disposable));
        }

        protected virtual void DisposeRenderDisposables(Window window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            var win = window.Window;
            if (win == null)
                return;

            var dd = _windowsResources[win]._renderDisposables;
            do
            {
                if (!dd.TryTake(out var obj))
                    break;

                obj.BaseDispose();
            }
            while (true);
        }

        internal void AddWindow(Window window)
        {
            if (_windowsResources.ContainsKey(window))
                return;

            var res = new WindowResources(window);
            res = _windowsResources.AddOrUpdate(window, res, (o, k) => k);
        }

        internal void RemoveWindow(Window window)
        {
            DisposeRenderDisposables(window);
        }

        private interface IKeyable
        {
            string Key { get; }
        }

        private class KeyComObject<T> : ComObject<T>, IKeyable
        {
            public KeyComObject(T comObject, string key)
                : base(comObject)
            {
                Key = key;
            }

            public string Key { get; }
        }

        private interface IBaseDisposable
        {
            void BaseDispose();
        }

        private class RenderComObject<T> : ComObject<T>, IBaseDisposable
        {
            private readonly Window _window;
            private readonly ResourceManager _mgr;

            public RenderComObject(ResourceManager mgr, Window window, T comObject)
                : base(comObject)
            {
                _mgr = mgr;
                _window = window;
            }

            // don't call the real dispose now
            protected override void Dispose(bool disposing) => _mgr._windowsResources[_window]._renderDisposables.Add(this);
            public void BaseDispose() => base.Dispose(true);
        }

        private class RenderDisposable : IBaseDisposable
        {
            private readonly IDisposable _disposable;

            public RenderDisposable(IDisposable disposable)
            {
                _disposable = disposable;
            }

            public void BaseDispose() => _disposable.Dispose();
            public override string ToString() => _disposable.ToString();
        }

        private class WindowResources
        {
            public WindowResources(Window window)
            {
                Window = window;
            }

            public Window Window;
            public readonly ConcurrentBag<IBaseDisposable> _renderDisposables = new ConcurrentBag<IBaseDisposable>();
            public readonly ConcurrentDictionary<string, Resource> _resources = new ConcurrentDictionary<string, Resource>(StringComparer.OrdinalIgnoreCase);

            public void DisposeResources()
            {
                foreach (var kv in _resources.ToArray())
                {
                    kv.Value.Dispose();
                }
                // note: possible race condition here
                _resources.Clear();
            }
        }

        private class Resource : IDisposable
        {
            public Resource()
            {
                LastAccess = DateTime.Now;
            }

            public DateTime LastAccess;
            public object Object;

            public void Dispose()
            {
                //Application.Trace("Dispose " + Object + " " + LastAccess + " Elapsed: " + (DateTime.Now - LastAccess).ToString());
                ((IDisposable)Object)?.Dispose();
            }

            public override string ToString() => LastAccess + " " + Object?.ToString();
        }
    }
}
