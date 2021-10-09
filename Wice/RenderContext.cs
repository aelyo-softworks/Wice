using System;
using System.Runtime.InteropServices;
using DirectN;

namespace Wice
{
    public class RenderContext
    {
        public bool IsOverChildren { get; internal set; }
        public IComObject<ID2D1DeviceContext> DeviceContext { get; internal set; }
        public SurfaceCreationOptions SurfaceCreationOptions { get; internal set; }
        public tagRECT? SurfaceRect { get; internal set; }

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
