using System;

namespace DirectN
{
    public static class ID2D1GeometrySinkExtensions
    {
        public static void AddArc(this IComObject<ID2D1GeometrySink> sink, D2D_POINT_2F endPoint, D2D_SIZE_F radiusSize, float rotationAngle = 0, D2D1_SWEEP_DIRECTION sweepDirection = D2D1_SWEEP_DIRECTION.D2D1_SWEEP_DIRECTION_COUNTER_CLOCKWISE, D2D1_ARC_SIZE arcSize = D2D1_ARC_SIZE.D2D1_ARC_SIZE_SMALL) => AddArc(sink?.Object, endPoint, radiusSize, rotationAngle, sweepDirection, arcSize);
        public static void AddArc(this ID2D1GeometrySink sink, D2D_POINT_2F endPoint, D2D_SIZE_F radiusSize, float rotationAngle = 0, D2D1_SWEEP_DIRECTION sweepDirection = D2D1_SWEEP_DIRECTION.D2D1_SWEEP_DIRECTION_COUNTER_CLOCKWISE, D2D1_ARC_SIZE arcSize = D2D1_ARC_SIZE.D2D1_ARC_SIZE_SMALL)
        {
            if (sink == null)
                throw new ArgumentNullException(nameof(sink));

            var seg = new D2D1_ARC_SEGMENT();
            seg.point = endPoint;
            seg.size = radiusSize;
            seg.rotationAngle = rotationAngle;
            seg.sweepDirection = sweepDirection;
            seg.arcSize = arcSize;
            sink.AddArc(ref seg);
        }
    }
}
