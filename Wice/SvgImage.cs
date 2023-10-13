﻿using System;
using System.ComponentModel;
using System.Threading;
using DirectN;
using Wice.Utilities;

namespace Wice
{
    public class SvgImage : RenderVisual, IDisposable
    {
        public static VisualProperty DocumentProperty = VisualProperty.Add<IReadStreamer>(typeof(SvgImage), nameof(Document), VisualPropertyInvalidateModes.Measure);
        public static VisualProperty StretchProperty = VisualProperty.Add(typeof(SvgImage), nameof(Stretch), VisualPropertyInvalidateModes.Measure, Stretch.Uniform);
        public static VisualProperty StretchDirectionProperty = VisualProperty.Add(typeof(SvgImage), nameof(StretchDirection), VisualPropertyInvalidateModes.Measure, StretchDirection.Both);

        private bool _disposedValue;
        private UnmanagedMemoryStream _documentBuffer;
        private bool _bufferStream;

        public SvgImage()
        {
            BackgroundColor = _D3DCOLORVALUE.Transparent;
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
        public IReadStreamer Document { get => (IReadStreamer)GetPropertyValue(DocumentProperty); set => SetPropertyValue(DocumentProperty, value); }

        [Category(CategoryLayout)]
        public Stretch Stretch { get => (Stretch)GetPropertyValue(StretchProperty); set => SetPropertyValue(StretchProperty, value); }

        [Category(CategoryLayout)]
        public StretchDirection StretchDirection { get => (StretchDirection)GetPropertyValue(StretchDirectionProperty); set => SetPropertyValue(StretchDirectionProperty, value); }

        protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
        {
            if (!base.SetPropertyValue(property, value, options))
                return false;

            if (property == DocumentProperty)
            {
                Interlocked.Exchange(ref _documentBuffer, null)?.Dispose();
            }
            return true;
        }

        protected internal override void RenderCore(RenderContext context)
        {
            base.RenderCore(context);
            var doc = Document;
            if (doc == null)
                return;

            var dc = context.DeviceContext.Object.ComCast<ID2D1DeviceContext5>(false);
            if (dc == null)
                return;

            var rc = Image.GetDestinationRectangle(
                RenderSize,
                HorizontalAlignment,
                VerticalAlignment,
                Stretch,
                StretchDirection,
                RelativeRenderRect);

            //
            // note we cannot use D2D transforms on a SVG in a Direct Composition context, like this:
            //
            //    context.DeviceContext.Object.SetTransform(ref xf);
            //
            // as this causes strange flickering issues
            // so we have to create an svg document each render time (mostly due to resize)
            // hence the possible buffer feature
            //

            ID2D1SvgDocument svg = null;
            if (BufferStream)
            {
                if (_documentBuffer == null)
                {
                    using (var stream = doc.GetReadStream())
                    {
                        if (stream != null)
                        {
                            _documentBuffer = new UnmanagedMemoryStream(stream);
                            //Application.Trace("loaded buffer size:" + _documentBuffer.Length);
                        }
                    }
                }

                if (_documentBuffer.Length > 0)
                {
                    _documentBuffer.Position = 0;
                    dc.CreateSvgDocument(_documentBuffer, rc.Size, out svg).ThrowOnError();
                    //Application.Trace("loaded doc from buffer");
                }
            }
            else
            {
                using (var stream = doc.GetReadStream())
                {
                    if (stream != null)
                    {
                        dc.CreateSvgDocument(new ManagedIStream(stream), rc.Size, out svg).ThrowOnError();
                        //Application.Trace("loaded doc");
                    }
                }
            }
            if (svg == null)
                return;

            using (var document = new ComObject<ID2D1SvgDocument>(svg))
            {
                //Application.Trace("draw");
                dc.DrawSvgDocument(document.Object);
            }
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

        ~SvgImage() { Dispose(disposing: false); }
        public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
    }
}
