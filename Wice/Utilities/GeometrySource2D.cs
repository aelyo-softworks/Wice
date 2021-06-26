using System;
using DirectN;
using Windows.Graphics;

namespace Wice.Utilities
{
    public sealed class GeometrySource2D : IGeometrySource2D, IGeometrySource2DInterop, IEquatable<GeometrySource2D>
    {
        public GeometrySource2D(string uniqueKey)
        {
            if (uniqueKey == null)
                throw new ArgumentNullException(nameof(uniqueKey));

            UniqueKey = uniqueKey;
        }

        public string UniqueKey { get; }
        public ID2D1Geometry Geometry { get; set; }

        HRESULT IGeometrySource2DInterop.TryGetGeometryUsingFactory(ID2D1Factory factory, out ID2D1Geometry value) => throw new NotSupportedException();
        HRESULT IGeometrySource2DInterop.GetGeometry(out ID2D1Geometry value)
        {
            value = Geometry;
            return HRESULTS.S_OK;
        }

        public override int GetHashCode() => UniqueKey.GetHashCode();
        public override bool Equals(object obj) => Equals(obj as GeometrySource2D);
        public bool Equals(GeometrySource2D other)
        {
            if (other == null)
                return false;

            if (Equals(Geometry, other.Geometry))
                return true;

            return UniqueKey == other.UniqueKey;
        }
    }
}
