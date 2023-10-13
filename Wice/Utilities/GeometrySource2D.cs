using System;
using System.Runtime.InteropServices;
using DirectN;
#if NET
using IGeometrySource2D = Wice.Interop.IGeometrySource2DWinRT;
#else
using IGeometrySource2D = Windows.Graphics.IGeometrySource2D;
#endif

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

#if NET
        HRESULT IInspectable.GetIids(out int iidCount, out IntPtr iids)
        {
            iidCount = 0;
            iids = IntPtr.Zero;
            return HRESULTS.S_OK;
        }

        HRESULT IInspectable.GetRuntimeClassName(out string className)
        {
            className = null;
            return HRESULTS.S_OK;
        }

        HRESULT IInspectable.GetTrustLevel(out TrustLevel trustLevel)
        {
            trustLevel = TrustLevel.FullTrust;
            return HRESULTS.S_OK;
        }
#endif

        HRESULT IGeometrySource2DInterop.TryGetGeometryUsingFactory(ID2D1Factory factory, out ID2D1Geometry value) => throw new NotSupportedException();
        HRESULT IGeometrySource2DInterop.GetGeometry(out ID2D1Geometry value)
        {
            value = Geometry;
            return HRESULTS.S_OK;
        }

        public Windows.Graphics.IGeometrySource2D GetIGeometrySource2()
        {
#if NET
            var unk = Marshal.GetIUnknownForObject(this);
            return WinRT.MarshalInspectable<Windows.Graphics.IGeometrySource2D>.FromAbi(unk);

#else
            return this;
#endif
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
