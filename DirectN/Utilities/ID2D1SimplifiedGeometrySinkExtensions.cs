using System;
using System.Collections.Generic;
using System.Linq;

namespace DirectN
{
    public static class ID2D1SimplifiedGeometrySinkExtensions
    {
        public static void AddBeziers(this IComObject<ID2D1SimplifiedGeometrySink> sink, params D2D1_BEZIER_SEGMENT[] segments) => AddBeziers(sink?.Object, segments);
        public static void AddBeziers(this IComObject<ID2D1SimplifiedGeometrySink> sink, IEnumerable<D2D1_BEZIER_SEGMENT> segments) => AddBeziers(sink?.Object, segments);
        public static void AddBeziers(this ID2D1SimplifiedGeometrySink sink, IEnumerable<D2D1_BEZIER_SEGMENT> segments)
        {
            if (sink == null)
                throw new ArgumentNullException(nameof(sink));

            if (segments == null)
                throw new ArgumentNullException(nameof(segments));

            var array = segments.ToArray();
            if (array.Length == 0)
                throw new ArgumentException(null, nameof(segments));

            sink.AddBeziers(array, array.Length);
        }

        public static void AddLines(this IComObject<ID2D1SimplifiedGeometrySink> sink, params D2D_POINT_2F[] points) => AddLines(sink?.Object, points);
        public static void AddLines(this IComObject<ID2D1SimplifiedGeometrySink> sink, IEnumerable<D2D_POINT_2F> points) => AddLines(sink?.Object, points);
        public static void AddLines(this ID2D1SimplifiedGeometrySink sink, IEnumerable<D2D_POINT_2F> points)
        {
            if (sink == null)
                throw new ArgumentNullException(nameof(sink));

            if (points == null)
                throw new ArgumentNullException(nameof(points));

            var array = points.ToArray();
            if (array.Length == 0)
                throw new ArgumentException(null, nameof(points));

            sink.AddLines(array, array.Length);
        }

        public static void AddLine(this IComObject<ID2D1SimplifiedGeometrySink> sink, D2D_POINT_2F point) => AddLine(sink?.Object, point);
        public static void AddLine(this ID2D1SimplifiedGeometrySink sink, D2D_POINT_2F point)
        {
            if (sink == null)
                throw new ArgumentNullException(nameof(sink));

            if (sink is ID2D1GeometrySink gsink)
            {
                gsink.AddLine(point);
            }
            else
            {
                sink.AddLines(new[] { point }, 1);
            }
        }
    }
}