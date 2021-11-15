using System;

namespace DirectN
{
    public static class ID2D1RenderTargetExtensions
    {
        public static IComObject<ID2D1Bitmap> GetBitmap(this IComObject<ID2D1BitmapRenderTarget> renderTarget) => GetBitmap<ID2D1Bitmap>(renderTarget?.Object);
        public static IComObject<T> GetBitmap<T>(this IComObject<ID2D1BitmapRenderTarget> renderTarget) where T : ID2D1Bitmap => GetBitmap<T>(renderTarget?.Object);
        public static IComObject<T> GetBitmap<T>(this ID2D1BitmapRenderTarget renderTarget) where T : ID2D1Bitmap
        {
            if (renderTarget == null)
                throw new ArgumentNullException(nameof(renderTarget));

            renderTarget.GetBitmap(out var bmp).ThrowOnError();
            return new ComObject<T>((T)bmp);
        }

        public static IComObject<ID2D1Bitmap> CreateBitmap(this IComObject<ID2D1RenderTarget> renderTarget, D2D_SIZE_U size, D2D1_BITMAP_PROPERTIES properties) => CreateBitmap<ID2D1Bitmap>(renderTarget?.Object, size, IntPtr.Zero, 0, properties);
        public static IComObject<ID2D1Bitmap> CreateBitmap(this IComObject<ID2D1RenderTarget> renderTarget, D2D_SIZE_U size, IntPtr srcData, uint pitch, D2D1_BITMAP_PROPERTIES properties) => CreateBitmap<ID2D1Bitmap>(renderTarget?.Object, size, srcData, pitch, properties);
        public static IComObject<T> CreateBitmap<T>(this IComObject<ID2D1RenderTarget> renderTarget, D2D_SIZE_U size, IntPtr srcData, uint pitch, D2D1_BITMAP_PROPERTIES properties) where T : ID2D1Bitmap => CreateBitmap<T>(renderTarget?.Object, size, srcData, pitch, properties);
        public static IComObject<T> CreateBitmap<T>(this ID2D1RenderTarget renderTarget, D2D_SIZE_U size, IntPtr srcData, uint pitch, D2D1_BITMAP_PROPERTIES properties) where T : ID2D1Bitmap
        {
            if (renderTarget == null)
                throw new ArgumentNullException(nameof(renderTarget));

            renderTarget.CreateBitmap(size, srcData, pitch, ref properties, out var bmp).ThrowOnError();
            return new ComObject<T>((T)bmp);
        }

        public static IComObject<ID2D1SolidColorBrush> CreateSolidColorBrush(this IComObject<ID2D1RenderTarget> renderTarget, _D3DCOLORVALUE color, D2D1_BRUSH_PROPERTIES? properties = null) => CreateSolidColorBrush<ID2D1SolidColorBrush>(renderTarget?.Object, color, properties);
        public static IComObject<T> CreateSolidColorBrush<T>(this IComObject<ID2D1RenderTarget> renderTarget, _D3DCOLORVALUE color, D2D1_BRUSH_PROPERTIES? properties = null) where T : ID2D1SolidColorBrush => CreateSolidColorBrush<T>(renderTarget?.Object, color, properties);
        public static IComObject<T> CreateSolidColorBrush<T>(this ID2D1RenderTarget renderTarget, _D3DCOLORVALUE color, D2D1_BRUSH_PROPERTIES? properties = null) where T : ID2D1SolidColorBrush
        {
            if (renderTarget == null)
                throw new ArgumentNullException(nameof(renderTarget));

            if (properties.HasValue)
            {
                using (var props = properties.Value.StructureToMemory())
                {
                    renderTarget.CreateSolidColorBrush(ref color, props.Pointer, out var brush).ThrowOnError();
                    return new ComObject<T>((T)brush);
                }
            }

            renderTarget.CreateSolidColorBrush(ref color, IntPtr.Zero, out var brush2).ThrowOnError();
            return new ComObject<T>((T)brush2);
        }

        // this one is useless but is consistent with EndDraw
        public static void BeginDraw(this IComObject<ID2D1RenderTarget> renderTarget) => BeginDraw(renderTarget?.Object);
        public static void BeginDraw(this ID2D1RenderTarget renderTarget) => renderTarget?.BeginDraw();
        public static void EndDraw(this IComObject<ID2D1RenderTarget> renderTarget) => EndDraw(renderTarget?.Object);
        public static void EndDraw(this ID2D1RenderTarget renderTarget) => renderTarget?.EndDraw(IntPtr.Zero, IntPtr.Zero).ThrowOnError();
        public static void Clear(this IComObject<ID2D1RenderTarget> renderTarget, _D3DCOLORVALUE? clearColor = null) => Clear(renderTarget?.Object, clearColor);
        public static void Clear(this ID2D1RenderTarget renderTarget, _D3DCOLORVALUE? clearColor = null)
        {
            if (renderTarget == null)
                throw new ArgumentNullException(nameof(renderTarget));

            if (clearColor.HasValue)
            {
                using (var cc = clearColor.Value.StructureToMemory())
                {
                    renderTarget.Clear(cc.Pointer);
                }
                return;
            }
            renderTarget.Clear(IntPtr.Zero);
        }

        public static void DrawText(this IComObject<ID2D1RenderTarget> renderTarget,
            string text,
            IComObject<IDWriteTextFormat> format,
            D2D_RECT_F rect,
            IComObject<ID2D1Brush> brush,
            D2D1_DRAW_TEXT_OPTIONS options = D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_NONE,
            DWRITE_MEASURING_MODE measuringMode = DWRITE_MEASURING_MODE.DWRITE_MEASURING_MODE_NATURAL) => DrawText(renderTarget?.Object, text, format?.Object, rect, brush?.Object, options, measuringMode);

        public static void DrawText(this ID2D1RenderTarget renderTarget,
            string text,
            IDWriteTextFormat format,
            D2D_RECT_F rect,
            ID2D1Brush brush,
            D2D1_DRAW_TEXT_OPTIONS options = D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_NONE,
            DWRITE_MEASURING_MODE measuringMode = DWRITE_MEASURING_MODE.DWRITE_MEASURING_MODE_NATURAL)
        {
            if (renderTarget == null)
                throw new ArgumentNullException(nameof(renderTarget));

            if (format == null)
                throw new ArgumentNullException(nameof(format));

            if (text == null)
                return;

            renderTarget.DrawTextW(text, (uint)text.Length, format, ref rect, brush, options, measuringMode);
        }

        public static void DrawBitmap(this IComObject<ID2D1RenderTarget> renderTarget,
            IComObject<ID2D1Bitmap1> bitmap,
            float opacity,
            D2D1_BITMAP_INTERPOLATION_MODE interpolationMode,
            D2D_RECT_F? destinationRectangle = null,
            D2D_RECT_F? sourceRectangle = null) => DrawBitmap(renderTarget?.Object, bitmap?.Object, opacity, interpolationMode, destinationRectangle, sourceRectangle);

        public static void DrawBitmap(this IComObject<ID2D1RenderTarget> renderTarget,
            IComObject<ID2D1Bitmap> bitmap,
            float opacity,
            D2D1_BITMAP_INTERPOLATION_MODE interpolationMode,
            D2D_RECT_F? destinationRectangle = null,
            D2D_RECT_F? sourceRectangle = null) => DrawBitmap(renderTarget?.Object, bitmap?.Object, opacity, interpolationMode, destinationRectangle, sourceRectangle);

        public static void DrawBitmap(this ID2D1RenderTarget renderTarget,
            ID2D1Bitmap bitmap,
            float opacity,
            D2D1_BITMAP_INTERPOLATION_MODE interpolationMode,
            D2D_RECT_F? destinationRectangle = null,
            D2D_RECT_F? sourceRectangle = null)
        {
            if (renderTarget == null)
                throw new ArgumentNullException(nameof(renderTarget));

            using (var drc = destinationRectangle.StructureToMemory())
            {
                using (var src = sourceRectangle.StructureToMemory())
                {
                    renderTarget.DrawBitmap(bitmap, drc.Pointer, opacity, interpolationMode, src.Pointer);
                }
            }
        }

        public static void DrawGeometry(this IComObject<ID2D1RenderTarget> renderTarget,
            IComObject<ID2D1Geometry> geometry,
            IComObject<ID2D1Brush> brush,
            float strokeWidth,
            IComObject<ID2D1StrokeStyle> strokeStyle = null) => DrawGeometry(renderTarget?.Object, geometry?.Object, brush?.Object, strokeWidth, strokeStyle?.Object);

        public static void DrawGeometry(this ID2D1RenderTarget renderTarget,
            ID2D1Geometry geometry,
            ID2D1Brush brush,
            float strokeWidth,
            ID2D1StrokeStyle strokeStyle = null)
        {
            if (renderTarget == null)
                throw new ArgumentNullException(nameof(renderTarget));

            if (brush == null)
                throw new ArgumentNullException(nameof(brush));

            renderTarget.DrawGeometry(geometry, brush, strokeWidth, strokeStyle);
        }

        public static void PushAxisAlignedClip(this IComObject<ID2D1RenderTarget> renderTarget, D2D_RECT_F clipRect, D2D1_ANTIALIAS_MODE antialiasMode = D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_PER_PRIMITIVE) => PushAxisAlignedClip(renderTarget?.Object, clipRect, antialiasMode);
        public static void PushAxisAlignedClip(this ID2D1RenderTarget renderTarget, D2D_RECT_F clipRect, D2D1_ANTIALIAS_MODE antialiasMode = D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_PER_PRIMITIVE)
        {
            if (renderTarget == null)
                throw new ArgumentNullException(nameof(renderTarget));

            renderTarget.PushAxisAlignedClip(ref clipRect, antialiasMode);
        }

        public static void PopAxisAlignedClip(this IComObject<ID2D1RenderTarget> renderTarget) => PopAxisAlignedClip(renderTarget?.Object);
        public static void PopAxisAlignedClip(this ID2D1RenderTarget renderTarget)
        {
            if (renderTarget == null)
                throw new ArgumentNullException(nameof(renderTarget));

            renderTarget.PopAxisAlignedClip();
        }
    }
}