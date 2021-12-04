using System;
using System.ComponentModel;
using System.Threading;
using DirectN;

namespace Wice
{
    public class Image : RenderVisual, IDisposable
    {
        public static VisualProperty SourceOpacityProperty = VisualProperty.Add(typeof(Image), nameof(SourceOpacity), VisualPropertyInvalidateModes.Render, 1f);
        public static VisualProperty SourceProperty = VisualProperty.Add<IComObject<IWICBitmapSource>>(typeof(Image), nameof(Source), VisualPropertyInvalidateModes.Measure);
        public static VisualProperty InterpolationModeProperty = VisualProperty.Add(typeof(Image), nameof(InterpolationMode), VisualPropertyInvalidateModes.Render, D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR);
        public static VisualProperty StretchProperty = VisualProperty.Add(typeof(Image), nameof(Stretch), VisualPropertyInvalidateModes.Measure, Stretch.Uniform);
        public static VisualProperty StretchDirectionProperty = VisualProperty.Add(typeof(Image), nameof(StretchDirection), VisualPropertyInvalidateModes.Measure, StretchDirection.Both);
        public static VisualProperty SourceRectangleProperty = VisualProperty.Add<D2D_RECT_F?>(typeof(Image), nameof(SourceRectangle), VisualPropertyInvalidateModes.Measure);

        private bool _disposedValue;
        private IComObject<ID2D1Bitmap> _bitmap;

        public Image()
        {
            BackgroundColor = _D3DCOLORVALUE.Transparent;
        }

        [Category(CategoryBehavior)]
        public IComObject<IWICBitmapSource> Source { get => (IComObject<IWICBitmapSource>)GetPropertyValue(SourceProperty); set => SetPropertyValue(SourceProperty, value); }

        [Category(CategoryRender)]
        public float SourceOpacity { get => (float)GetPropertyValue(SourceOpacityProperty); set => SetPropertyValue(SourceOpacityProperty, value); }

        [Category(CategoryLayout)]
        public Stretch Stretch { get => (Stretch)GetPropertyValue(StretchProperty); set => SetPropertyValue(StretchProperty, value); }

        [Category(CategoryLayout)]
        public StretchDirection StretchDirection { get => (StretchDirection)GetPropertyValue(StretchDirectionProperty); set => SetPropertyValue(StretchDirectionProperty, value); }

        [Category(CategoryRender)]
        public D2D1_INTERPOLATION_MODE InterpolationMode { get => (D2D1_INTERPOLATION_MODE)GetPropertyValue(InterpolationModeProperty); set => SetPropertyValue(InterpolationModeProperty, value); }

        [Category(CategoryLayout)]
        public D2D_RECT_F? SourceRectangle { get => (D2D_RECT_F?)GetPropertyValue(SourceRectangleProperty); set => SetPropertyValue(SourceRectangleProperty, value); }

        protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint) => GetSize(constraint);

        protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
        {
            if (!base.SetPropertyValue(property, value, options))
                return false;

            if (property == SourceProperty)
            {
                Interlocked.Exchange(ref _bitmap, null)?.Dispose();
            }
            return true;
        }

        private D2D_SIZE_F GetSize(D2D_SIZE_F constraint)
        {
            var width = 0f;
            var height = 0f;

            var src = Source;
            if (src != null && !src.IsDisposed)
            {
                var size = src.GetSize();
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

        internal static D2D_RECT_F GetDestinationRectangle(
            D2D_SIZE_F size,
            Alignment horizontalAlignment,
            Alignment verticalAlignment,
            Stretch stretch,
            StretchDirection stretchDirection,
            D2D_RECT_F renderRect)
        {
            // stretch takes priority with regards to h/v aligments
            // if the bitmap doesn't strech, it uses it's source's size

            var rr = renderRect;
            var factor = GetScaleFactor(rr.Size, size, stretch, stretchDirection);
            var destRc = new D2D_RECT_F(0, 0, size.width * factor.width, size.height * factor.height);

            var ha = horizontalAlignment;
            float w;
            float h;
            switch (ha)
            {
                case Alignment.Center:
                case Alignment.Stretch:
                    w = (rr.Width - destRc.Width) / 2f;
                    destRc.left += w;
                    destRc.right += w;
                    break;

                case Alignment.Far:
                    w = rr.Width - destRc.Width;
                    destRc.left += w;
                    if (destRc.left < 0)
                    {
                        destRc.left = 0;
                    }
                    destRc.right += w;
                    break;

                case Alignment.Near:
                    break;
            }

            var va = verticalAlignment;
            switch (va)
            {
                case Alignment.Center:
                case Alignment.Stretch:
                    h = (rr.Height - destRc.Height) / 2f;
                    destRc.top += h;
                    destRc.bottom += h;
                    break;

                case Alignment.Far:
                    h = rr.Height - destRc.Height;
                    destRc.top += h;
                    if (destRc.top < 0)
                    {
                        destRc.top = 0;
                    }
                    destRc.bottom += h;
                    break;

                case Alignment.Near:
                    break;
            }
            return destRc;
        }

        protected internal override void RenderCore(RenderContext context)
        {
            base.RenderCore(context);
            var src = Source;
            if (src != null && !src.IsDisposed)
            {
                var bmp = _bitmap;
                if (bmp == null)
                {
                    context.DeviceContext.Object.CreateBitmapFromWicBitmap(Source.Object, IntPtr.Zero, out ID2D1Bitmap bitmap).ThrowOnError();
                    bmp = new ComObject<ID2D1Bitmap>(bitmap);
                    _bitmap = bmp;
                }

                if (bmp != null && !bmp.IsDisposed)
                {
                    var destRc = GetDestinationRectangle(
                        src.GetSizeF(),
                        HorizontalAlignment,
                        VerticalAlignment,
                        Stretch,
                        StretchDirection,
                        RelativeRenderRect);
                    var srcRectangle = SourceRectangle;
                    context.DeviceContext.DrawBitmap(bmp, SourceOpacity, InterpolationMode, destRc, srcRectangle);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    Interlocked.Exchange(ref _bitmap, null)?.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                _disposedValue = true;
            }
        }

        ~Image() { Dispose(disposing: false); }
        public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
    }
}