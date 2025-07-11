﻿namespace Wice;

public partial class Image : RenderVisual, IDisposable
{
    public static VisualProperty SourceOpacityProperty { get; } = VisualProperty.Add(typeof(Image), nameof(SourceOpacity), VisualPropertyInvalidateModes.Render, 1f);
    public static VisualProperty SourceProperty { get; } = VisualProperty.Add<IComObject<IWICBitmapSource>>(typeof(Image), nameof(Source), VisualPropertyInvalidateModes.Measure);
    public static VisualProperty InterpolationModeProperty { get; } = VisualProperty.Add(typeof(Image), nameof(InterpolationMode), VisualPropertyInvalidateModes.Render, D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR);
    public static VisualProperty StretchProperty { get; } = VisualProperty.Add(typeof(Image), nameof(Stretch), VisualPropertyInvalidateModes.Measure, Stretch.Uniform);
    public static VisualProperty StretchDirectionProperty { get; } = VisualProperty.Add(typeof(Image), nameof(StretchDirection), VisualPropertyInvalidateModes.Measure, StretchDirection.Both);
    public static VisualProperty SourceRectangleProperty { get; } = VisualProperty.Add<D2D_RECT_F?>(typeof(Image), nameof(SourceRectangle), VisualPropertyInvalidateModes.Measure);

    public event EventHandler<EventArgs>? BitmapCreated;
    public event EventHandler<EventArgs>? BitmapDisposed;
    public event EventHandler<EventArgs>? BitmapError;

    private bool _disposedValue;
    private IComObject<ID2D1Bitmap>? _bitmap;

    public Image()
    {
        BackgroundColor = D3DCOLORVALUE.Transparent;
    }

    public IComObject<ID2D1Bitmap>? Bitmap => _bitmap;
    public virtual HRESULT LastBitmapError { get; protected set; }

    [Category(CategoryBehavior)]
    public IComObject<IWICBitmapSource>? Source { get => (IComObject<IWICBitmapSource>?)GetPropertyValue(SourceProperty); set => SetPropertyValue(SourceProperty, value); }

    [Category(CategoryRender)]
    public float SourceOpacity { get => (float)GetPropertyValue(SourceOpacityProperty)!; set => SetPropertyValue(SourceOpacityProperty, value); }

    [Category(CategoryLayout)]
    public Stretch Stretch { get => (Stretch)GetPropertyValue(StretchProperty)!; set => SetPropertyValue(StretchProperty, value); }

    [Category(CategoryLayout)]
    public StretchDirection StretchDirection { get => (StretchDirection)GetPropertyValue(StretchDirectionProperty)!; set => SetPropertyValue(StretchDirectionProperty, value); }

    [Category(CategoryRender)]
    public D2D1_INTERPOLATION_MODE InterpolationMode { get => (D2D1_INTERPOLATION_MODE)GetPropertyValue(InterpolationModeProperty)!; set => SetPropertyValue(InterpolationModeProperty, value); }

    [Category(CategoryLayout)]
    public D2D_RECT_F? SourceRectangle { get => (D2D_RECT_F?)GetPropertyValue(SourceRectangleProperty); set => SetPropertyValue(SourceRectangleProperty, value); }

    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint) => GetSize(constraint);

    protected override bool SetPropertyValue(BaseObjectProperty property, object? value, BaseObjectSetOptions? options = null)
    {
        if (!base.SetPropertyValue(property, value, options))
            return false;

        if (property == SourceProperty)
        {
            DisposeBitmap();
        }
        return true;
    }

    protected virtual D2D_SIZE_F GetSize(D2D_SIZE_F constraint)
    {
        var width = 0f;
        var height = 0f;

        var src = Source;
        if (src != null && !src.IsDisposed)
        {
            var size = src.GetSizeF();
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

    public virtual D2D_RECT_F GetDestinationRectangle() => GetDestinationRectangle(
                    Source?.GetSizeF() ?? new D2D_SIZE_F(),
                    HorizontalAlignment,
                    VerticalAlignment,
                    Stretch,
                    StretchDirection,
                    RelativeRenderRect);

    public static D2D_RECT_F GetDestinationRectangle(
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
            if (bmp == null && Source != null)
            {
                LastBitmapError = context.DeviceContext.Object.CreateBitmapFromWicBitmap(Source.Object, IntPtr.Zero, out ID2D1Bitmap bitmap);
                if (LastBitmapError.IsError)
                {
                    OnBitmapError(this, EventArgs.Empty);
                }
                else
                {
                    bmp = new ComObject<ID2D1Bitmap>(bitmap);
                    _bitmap = bmp;
                    OnBitmapCreated(this, EventArgs.Empty);
                }
            }

            if (bmp != null && !bmp.IsDisposed)
            {
                var destRc = GetDestinationRectangle();
                var srcRectangle = SourceRectangle;
                context.DeviceContext.DrawBitmap(bmp, SourceOpacity, InterpolationMode, destRc, srcRectangle);
            }
        }
    }

    protected virtual void DisposeBitmap()
    {
        var bmp = Interlocked.Exchange(ref _bitmap, null);
        if (bmp != null)
        {
            bmp.Dispose();
            OnBitmapDisposed(this, EventArgs.Empty);
        }
    }

    protected virtual void OnBitmapDisposed(object sender, EventArgs args) => BitmapDisposed?.Invoke(sender, args);
    protected virtual void OnBitmapCreated(object sender, EventArgs args) => BitmapCreated?.Invoke(sender, args);
    protected virtual void OnBitmapError(object sender, EventArgs args) => BitmapError?.Invoke(sender, args);

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                DisposeBitmap();
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            _disposedValue = true;
        }
    }

    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
}