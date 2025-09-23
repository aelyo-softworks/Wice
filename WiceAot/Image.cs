﻿namespace Wice;

/// <summary>
/// Renders a WIC bitmap source into the visual using Direct2D. Supports stretching, alignment,
/// interpolation, an optional source crop rectangle, and configurable source opacity.
/// </summary>
public partial class Image : RenderVisual, IDisposable
{
    /// <summary>
    /// Attached property storing the opacity applied to the source bitmap during drawing.
    /// Default is 1.0 (fully opaque). Changing this invalidates the render pass.
    /// </summary>
    public static VisualProperty SourceOpacityProperty { get; } = VisualProperty.Add(typeof(Image), nameof(SourceOpacity), VisualPropertyInvalidateModes.Render, 1f);

    /// <summary>
    /// Attached property containing the WIC bitmap source. Changing this invalidates measure
    /// (size can change) and triggers disposal of the cached D2D bitmap.
    /// </summary>
    public static VisualProperty SourceProperty { get; } = VisualProperty.Add<IComObject<IWICBitmapSource>>(typeof(Image), nameof(Source), VisualPropertyInvalidateModes.Measure);

    /// <summary>
    /// Attached property specifying the interpolation mode used when drawing the bitmap.
    /// Default is <see cref="D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR"/>. Changing this invalidates render.
    /// </summary>
    public static VisualProperty InterpolationModeProperty { get; } = VisualProperty.Add(typeof(Image), nameof(InterpolationMode), VisualPropertyInvalidateModes.Render, D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR);

    /// <summary>
    /// Attached property controlling how the image scales to fit the available space.
    /// Default is <see cref="Stretch.Uniform"/>. Changing this invalidates measure.
    /// </summary>
    public static VisualProperty StretchProperty { get; } = VisualProperty.Add(typeof(Image), nameof(Stretch), VisualPropertyInvalidateModes.Measure, Stretch.Uniform);

    /// <summary>
    /// Attached property determining whether scaling can grow, shrink, or both.
    /// Default is <see cref="StretchDirection.Both"/>. Changing this invalidates measure.
    /// </summary>
    public static VisualProperty StretchDirectionProperty { get; } = VisualProperty.Add(typeof(Image), nameof(StretchDirection), VisualPropertyInvalidateModes.Measure, StretchDirection.Both);

    /// <summary>
    /// Attached property defining an optional source crop rectangle (in source DIPs).
    /// When null, the full source bitmap is used. Changing this invalidates measure (affects layout if stretching).
    /// </summary>
    public static VisualProperty SourceRectangleProperty { get; } = VisualProperty.Add<D2D_RECT_F?>(typeof(Image), nameof(SourceRectangle), VisualPropertyInvalidateModes.Measure);

    /// <summary>
    /// Raised after the Direct2D bitmap is created from <see cref="Source"/> during rendering.
    /// </summary>
    public event EventHandler<EventArgs>? BitmapCreated;

    /// <summary>
    /// Occurs when the bitmap resource has been disposed.
    /// </summary>
    public event EventHandler<EventArgs>? BitmapDisposed;

    /// <summary>
    /// Raised when creating the Direct2D bitmap from <see cref="Source"/> fails during rendering.
    /// See <see cref="LastBitmapError"/> for details.
    /// </summary>
    public event EventHandler<EventArgs>? BitmapError;

    private bool _disposedValue;
    private IComObject<ID2D1Bitmap>? _bitmap;

    /// <summary>
    /// Initializes a new instance of <see cref="Image"/> setting a transparent background by default.
    /// </summary>
    public Image()
    {
        BackgroundColor = D3DCOLORVALUE.Transparent;
    }

    /// <summary>
    /// Gets the cached Direct2D bitmap created from <see cref="Source"/> (if created).
    /// May be null before first render or after disposal.
    /// </summary>
    public IComObject<ID2D1Bitmap>? Bitmap => _bitmap;

    /// <summary>
    /// Gets the last HRESULT returned when attempting to create the Direct2D bitmap from <see cref="Source"/>.
    /// A failing value indicates <see cref="BitmapError"/> was raised.
    /// </summary>
    public virtual HRESULT LastBitmapError { get; protected set; }

    /// <summary>
    /// Gets or sets the WIC bitmap source used to render the image. Setting this disposes any cached
    /// Direct2D bitmap and triggers a new measure pass.
    /// </summary>
    [Category(CategoryBehavior)]
    public IComObject<IWICBitmapSource>? Source { get => (IComObject<IWICBitmapSource>?)GetPropertyValue(SourceProperty); set => SetPropertyValue(SourceProperty, value); }

    /// <summary>
    /// Gets or sets the opacity applied to the source bitmap during drawing in the range [0..1].
    /// Default is 1.0 (opaque). Changing this invalidates render.
    /// </summary>
    [Category(CategoryRender)]
    public float SourceOpacity { get => (float)GetPropertyValue(SourceOpacityProperty)!; set => SetPropertyValue(SourceOpacityProperty, value); }

    /// <summary>
    /// Gets or sets how the image is scaled to fit the available space.
    /// Default is <see cref="Stretch.Uniform"/>. Changing this invalidates measure.
    /// </summary>
    [Category(CategoryLayout)]
    public Stretch Stretch { get => (Stretch)GetPropertyValue(StretchProperty)!; set => SetPropertyValue(StretchProperty, value); }

    /// <summary>
    /// Gets or sets whether scaling can grow, shrink, or both.
    /// Default is <see cref="StretchDirection.Both"/>. Changing this invalidates measure.
    /// </summary>
    [Category(CategoryLayout)]
    public StretchDirection StretchDirection { get => (StretchDirection)GetPropertyValue(StretchDirectionProperty)!; set => SetPropertyValue(StretchDirectionProperty, value); }

    /// <summary>
    /// Gets or sets the interpolation mode to use when drawing the bitmap.
    /// Default is linear interpolation. Changing this invalidates render.
    /// </summary>
    [Category(CategoryRender)]
    public D2D1_INTERPOLATION_MODE InterpolationMode { get => (D2D1_INTERPOLATION_MODE)GetPropertyValue(InterpolationModeProperty)!; set => SetPropertyValue(InterpolationModeProperty, value); }

    /// <summary>
    /// Gets or sets an optional source rectangle (in DIPs) used to crop the source prior to drawing.
    /// When null, the full source is used. Changing this invalidates measure.
    /// </summary>
    [Category(CategoryLayout)]
    public D2D_RECT_F? SourceRectangle { get => (D2D_RECT_F?)GetPropertyValue(SourceRectangleProperty); set => SetPropertyValue(SourceRectangleProperty, value); }

    /// <inheritdoc />
    protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint) => GetSize(constraint);

    /// <inheritdoc/>
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

    /// <summary>
    /// Computes the desired size for this image given the layout constraint and current stretch settings.
    /// Uses the natural source size when available and applies <see cref="Stretch"/> and <see cref="StretchDirection"/>.
    /// </summary>
    /// <param name="constraint">Available size provided by the parent (in DIPs).</param>
    /// <returns>The desired size for layout.</returns>
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

    /// <summary>
    /// Calculates the destination rectangle for rendering based on the source size, alignment, stretch mode, and other
    /// parameters.
    /// </summary>
    /// <returns>A <see cref="D2D_RECT_F"/> representing the calculated destination rectangle.</returns>
    public virtual D2D_RECT_F GetDestinationRectangle() => GetDestinationRectangle(
                    Source?.GetSizeF() ?? new D2D_SIZE_F(),
                    HorizontalAlignment,
                    VerticalAlignment,
                    Stretch,
                    StretchDirection,
                    RelativeRenderRect);

    /// <summary>
    /// Computes the destination rectangle for the given input parameters.
    /// Stretching takes precedence over alignment: if stretching occurs, the aligned rectangle
    /// is computed from the stretched size; otherwise, the natural source size is aligned.
    /// </summary>
    /// <param name="size">Natural content size (width/height in DIPs).</param>
    /// <param name="horizontalAlignment">Horizontal alignment behavior.</param>
    /// <param name="verticalAlignment">Vertical alignment behavior.</param>
    /// <param name="stretch">Stretch mode applied to the content.</param>
    /// <param name="stretchDirection">Constraints on shrinking/growing.</param>
    /// <param name="renderRect">The available render rectangle (relative to parent).</param>
    /// <returns>The destination rectangle relative to <paramref name="renderRect"/> origin.</returns>
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

    /// <inheritdoc/>
    protected internal override void RenderCore(RenderContext context)
    {
        base.RenderCore(context);
        var src = Source;
        if (src != null && !src.IsDisposed)
        {
            var bmp = _bitmap;
            if (bmp == null && Source != null)
            {
                // note Direct Composition authorizes use to save the bitmap and reuse it although the D2D device context is new
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

    /// <summary>
    /// Disposes and clears the cached Direct2D bitmap, raising <see cref="BitmapDisposed"/> if a bitmap was present.
    /// Thread-safe via <see cref="Interlocked.Exchange{T}(ref T, T)"/>.
    /// </summary>
    protected virtual void DisposeBitmap()
    {
        var bmp = Interlocked.Exchange(ref _bitmap, null);
        if (bmp != null)
        {
            bmp.Dispose();
            OnBitmapDisposed(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Raises <see cref="BitmapDisposed"/>.
    /// </summary>
    protected virtual void OnBitmapDisposed(object sender, EventArgs args) => BitmapDisposed?.Invoke(sender, args);

    /// <summary>
    /// Raises <see cref="BitmapCreated"/>.
    /// </summary>
    protected virtual void OnBitmapCreated(object sender, EventArgs args) => BitmapCreated?.Invoke(sender, args);

    /// <summary>
    /// Raises <see cref="BitmapError"/>.
    /// </summary>
    protected virtual void OnBitmapError(object sender, EventArgs args) => BitmapError?.Invoke(sender, args);

    /// <summary>
    /// Implements the standard dispose pattern. Disposes managed resources when <paramref name="disposing"/> is true,
    /// notably the cached Direct2D bitmap. Unmanaged resources would be released here as well.
    /// </summary>
    /// <param name="disposing">true when called from <see cref="Dispose()"/>; false when from a finalizer.</param>
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

    /// <summary>
    /// Disposes this instance and suppresses finalization.
    /// </summary>
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
}