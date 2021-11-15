using System;

namespace DirectN
{
    public static class ID2D1BitmapExtensions
    {
        public static D2D_SIZE_F GetDpi(this IComObject<ID2D1Bitmap> bitmap) => GetDpi(bitmap?.Object);
        public static D2D_SIZE_F GetDpi(this ID2D1Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            bitmap.GetDpi(out var dx, out var dy);
            return new D2D_SIZE_F(dx, dy);
        }

        public static D2D_SIZE_F GetSize(this IComObject<ID2D1Bitmap> bitmap) => GetSize(bitmap?.Object);
        public static D2D_SIZE_F GetSize(this ID2D1Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            bitmap.GetSize(out var size);
            return size;
        }

        public static D2D_SIZE_U GetPixelSize(this IComObject<ID2D1Bitmap> bitmap) => GetPixelSize(bitmap?.Object);
        public static D2D_SIZE_U GetPixelSize(this ID2D1Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            bitmap.GetPixelSize(out var size);
            return size;
        }

        public static D2D1_PIXEL_FORMAT GetPixelFormat(this IComObject<ID2D1Bitmap> bitmap) => GetPixelFormat(bitmap?.Object);
        public static D2D1_PIXEL_FORMAT GetPixelFormat(this ID2D1Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            bitmap.GetPixelFormat(out var format);
            return format;
        }

        public static void CopyFromRenderTarget(this IComObject<ID2D1Bitmap> bitmap, IComObject<ID2D1RenderTarget> renderTarget, D2D_POINT_2U? destPoint = null, D2D_RECT_U? srcRect = null) => CopyFromRenderTarget(bitmap?.Object, renderTarget?.Object, destPoint, srcRect);
        public static void CopyFromRenderTarget(this ID2D1Bitmap bitmap, ID2D1RenderTarget renderTarget, D2D_POINT_2U? destPoint = null, D2D_RECT_U? srcRect = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            if (renderTarget == null)
                throw new ArgumentNullException(nameof(renderTarget));

            using (var destPointMem = new ComMemory(destPoint))
            {
                using (var srcRectMem = new ComMemory(srcRect))
                {
                    bitmap.CopyFromRenderTarget(destPointMem.Pointer, renderTarget, srcRectMem.Pointer).ThrowOnError();
                }
            }
        }

        public static void CopyFromMemory(this IComObject<ID2D1Bitmap> bitmap, IntPtr srcData, uint pitch, D2D_RECT_U? dstRect = null) => CopyFromMemory(bitmap?.Object, srcData, pitch, dstRect);
        public static void CopyFromMemory(this ID2D1Bitmap bitmap, IntPtr srcData, uint pitch, D2D_RECT_U? dstRect = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            if (srcData == IntPtr.Zero)
                throw new ArgumentException(null, nameof(srcData));

            using (var dstRectMem = new ComMemory(dstRect))
            {
                bitmap.CopyFromMemory(dstRectMem.Pointer, srcData, pitch).ThrowOnError();
            }
        }

        public static void CopyFromBitmap(this IComObject<ID2D1Bitmap> bitmap, IComObject<ID2D1Bitmap> copyBitmap, D2D_POINT_2U? destPoint = null, D2D_RECT_U? srcRect = null) => CopyFromBitmap(bitmap?.Object, copyBitmap?.Object, destPoint, srcRect);
        public static void CopyFromBitmap(this ID2D1Bitmap bitmap, ID2D1Bitmap copyBitmap, D2D_POINT_2U? destPoint = null, D2D_RECT_U? srcRect = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            if (copyBitmap == null)
                throw new ArgumentNullException(nameof(copyBitmap));

            using (var destPointMem = new ComMemory(destPoint))
            {
                using (var srcRectMem = new ComMemory(srcRect))
                {
                    bitmap.CopyFromBitmap(destPointMem.Pointer, copyBitmap, srcRectMem.Pointer).ThrowOnError();
                }
            }
        }
    }
}
