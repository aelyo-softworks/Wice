using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice
{
    public class RenderContext
    {
        private RenderContext(IComObject<ID2D1DeviceContext> deviceContext, SurfaceCreationOptions creationOptions = null, tagRECT? rect = null)
        {
            DeviceContext = deviceContext;
            SurfaceRect = rect;
            SurfaceCreationOptions = creationOptions;
        }

        public IComObject<ID2D1DeviceContext> DeviceContext { get; private set; }
        public SurfaceCreationOptions SurfaceCreationOptions { get; }
        public tagRECT? SurfaceRect { get; }

        public static void WithRenderContext(IComObject<ID2D1DeviceContext> deviceContext, Action<RenderContext> action, SurfaceCreationOptions creationOptions = null, tagRECT? rect = null)
        {
            if (deviceContext == null)
                throw new ArgumentNullException(nameof(deviceContext));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var rc = new RenderContext(deviceContext, creationOptions, rect);
            try
            {
                action(rc);
            }
            finally
            {
                rc.DeviceContext = null;
            }
        }

        public virtual IComObject<ID2D1Brush> CreateSolidColorBrush(_D3DCOLORVALUE? color)
        {
            if (!color.HasValue)
                return null;

            var value = color.Value;
            DeviceContext.Object.CreateSolidColorBrush(ref value, IntPtr.Zero, out var brush).ThrowOnError();
            return new ComObject<ID2D1Brush>(brush);
        }

        public virtual IComObject<T> CreateBitmapBrush<T>(ID2D1Bitmap bitmap, D2D1_BITMAP_BRUSH_PROPERTIES? bitmapBrushProperties = null, D2D1_BRUSH_PROPERTIES? brushProperties = null) where T : ID2D1BitmapBrush
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            using (var bprops = new ComMemory(bitmapBrushProperties))
            {
                using (var props = new ComMemory(brushProperties))
                {
                    DeviceContext.Object.CreateBitmapBrush(bitmap, bprops.Pointer, props.Pointer, out ID2D1BitmapBrush brush).ThrowOnError();
                    return new ComObject<T>((T)brush);
                }
            }
        }

        public virtual IComObject<ID2D1Brush> CreateLinearGradientBrush(D2D1_LINEAR_GRADIENT_BRUSH_PROPERTIES properties, params D2D1_GRADIENT_STOP[] stops)
        {
            if (stops.IsEmpty())
                throw new ArgumentException(null, nameof(stops));

            var value = properties;
            DeviceContext.Object.CreateGradientStopCollection(stops, stops.Length, D2D1_GAMMA.D2D1_GAMMA_2_2, D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP, out var coll).ThrowOnError();
            try
            {
                DeviceContext.Object.CreateLinearGradientBrush(ref value, IntPtr.Zero, coll, out var brush).ThrowOnError();
                return new ComObject<ID2D1Brush>(brush);
            }
            finally
            {
                if (coll != null)
                {
                    Marshal.ReleaseComObject(coll);
                }
            }
        }

        public IComObject<ID2D1Brush> CreateRadialGradientBrush(D2D1_RADIAL_GRADIENT_BRUSH_PROPERTIES properties, D2D1_GAMMA gamma = D2D1_GAMMA.D2D1_GAMMA_2_2, D2D1_EXTEND_MODE extendMode = D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP, params D2D1_GRADIENT_STOP[] stops)
        {
            if (stops.IsEmpty())
                throw new ArgumentException(null, nameof(stops));

            var value = properties;
            DeviceContext.Object.CreateGradientStopCollection(stops, stops.Length, gamma, extendMode, out var coll).ThrowOnError();
            try
            {
                DeviceContext.Object.CreateRadialGradientBrush(ref value, IntPtr.Zero, coll, out var brush).ThrowOnError();
                return new ComObject<ID2D1Brush>(brush);
            }
            finally
            {
                if (coll != null)
                {
                    Marshal.ReleaseComObject(coll);
                }
            }
        }
    }
}
