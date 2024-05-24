using System.Runtime.InteropServices.Marshalling;

namespace Wice.Utilities;

[GeneratedComClass]
public sealed partial class GeometrySource2D : IGeometrySource2D, IGeometrySource2DInterop, IEquatable<GeometrySource2D>, Interop.IInspectable
{
    public GeometrySource2D(string uniqueKey)
    {
        ArgumentNullException.ThrowIfNull(uniqueKey);
        UniqueKey = uniqueKey;
    }

    public string UniqueKey { get; }
    public ID2D1Geometry? Geometry { get; set; }

#if NET
    HRESULT Interop.IInspectable.GetIids(out uint iidCount, out nint iids)
    {
        iidCount = 0;
        iids = 0;
        return Constants.S_OK;
    }

    HRESULT Interop.IInspectable.GetRuntimeClassName(out HSTRING className)
    {
        className = HSTRING.Null;
        return Constants.S_OK;
    }

    HRESULT Interop.IInspectable.GetTrustLevel(out TrustLevel trustLevel)
    {
        trustLevel = TrustLevel.FullTrust;
        return Constants.S_OK;
    }
#endif

    HRESULT IGeometrySource2DInterop.TryGetGeometryUsingFactory(ID2D1Factory factory, out ID2D1Geometry value) => throw new NotSupportedException();
    HRESULT IGeometrySource2DInterop.GetGeometry(out ID2D1Geometry value)
    {
        value = Geometry;
        return Constants.S_OK;
    }

    public IGeometrySource2D GetIGeometrySource2()
    {
        ComWrappers.TryGetComInstance(this, out var unk);
        return WinRT.MarshalInspectable<IGeometrySource2D>.FromAbi(unk);
    }

    public override int GetHashCode() => UniqueKey.GetHashCode();
    public override bool Equals(object? obj) => Equals(obj as GeometrySource2D);
    public bool Equals(GeometrySource2D? other)
    {
        if (other == null)
            return false;

        if (Equals(Geometry, other.Geometry))
            return true;

        return UniqueKey == other.UniqueKey;
    }
}
