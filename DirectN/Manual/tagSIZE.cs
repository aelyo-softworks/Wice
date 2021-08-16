using System;
using System.Runtime.InteropServices;
using Windows.Foundation;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public struct tagSIZE
    {
        public int width;
        public int height;

        public tagSIZE(int width, int height)
        {
#if DEBUG
            if (width < 0)
                throw new ArgumentException(null, nameof(width));

            if (height < 0)
                throw new ArgumentException(null, nameof(height));
#endif

            this.width = width;
            this.height = height;
        }

        public tagSIZE(float width, float height)
        {
#if DEBUG
            if (width.IsInvalid())
                throw new ArgumentException(null, nameof(width));

            if (height.IsInvalid())
                throw new ArgumentException(null, nameof(height));

            if (width < 0)
                throw new ArgumentException(null, nameof(width));

            if (height < 0)
                throw new ArgumentException(null, nameof(height));
#endif

            this.width = width.FloorI();
            this.height = height.FloorI();

#if DEBUG
            if (this.width < 0)
                throw new ArgumentException(null, nameof(width));

            if (this.height < 0)
                throw new ArgumentException(null, nameof(height));
#endif
        }

        public bool IsZero => width == 0 && height == 0;
        public bool IsEmpty => width == 0 || height == 0;

        public override string ToString() => width + "," + height;
        public tagRECT ToRECT() => new tagRECT(0, 0, width, height);
        public D2D_SIZE_U ToD2D_SIZE_U() => new D2D_SIZE_U(width, height);
        public D2D_SIZE_F ToD2D_SIZE_F() => new D2D_SIZE_F(width, height);
        public Size ToSize() => new Size(width, height);

        public const long HIMETRIC_PER_INCH = 2540;

        public D2D_SIZE_F PixelToHiMetric()
        {
            var dpi = D2D1Functions.Dpi;
            return new D2D_SIZE_F(HIMETRIC_PER_INCH * width / dpi.width, HIMETRIC_PER_INCH * height / dpi.height);
        }

        public D2D_SIZE_F HiMetricToPixel()
        {
            var dpi = D2D1Functions.Dpi;
            return new D2D_SIZE_F(width * dpi.width / HIMETRIC_PER_INCH, height * dpi.height / HIMETRIC_PER_INCH);
        }
    }
}
