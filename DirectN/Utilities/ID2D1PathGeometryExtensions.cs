using System;

namespace DirectN
{
    public static class ID2D1PathGeometryExtensions
    {
        public static IComObject<ID2D1SimplifiedGeometrySink> Open(this IComObject<ID2D1PathGeometry> geometry) => Open<ID2D1SimplifiedGeometrySink>(geometry?.Object);
        public static IComObject<T> Open<T>(this IComObject<ID2D1PathGeometry> geometry) where T : ID2D1SimplifiedGeometrySink => Open<T>(geometry?.Object);
        public static IComObject<T> Open<T>(this ID2D1PathGeometry geometry) where T : ID2D1SimplifiedGeometrySink
        {
            if (geometry == null)
                throw new ArgumentNullException(nameof(geometry));

            geometry.Open(out var sink).ThrowOnError();
            return new ComObject<T>((T)sink);
        }
    }
}
