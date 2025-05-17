namespace Wice.Utilities;

public partial class GeometrySource2D : IGeometrySource2D, Windows.Graphics.IGeometrySource2DInterop, IEquatable<GeometrySource2D>
{
    public GeometrySource2D(string uniqueKey)
    {
        ArgumentNullException.ThrowIfNull(uniqueKey);
        UniqueKey = uniqueKey;
    }

    public string UniqueKey { get; }
    public ID2D1Geometry? Geometry { get; set; }

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

    HRESULT Windows.Graphics.IGeometrySource2DInterop.TryGetGeometryUsingFactory(ID2D1Factory factory, out ID2D1Geometry value) => throw new NotImplementedException();
    HRESULT Windows.Graphics.IGeometrySource2DInterop.GetGeometry(out ID2D1Geometry value)
    {
        value = Geometry!;
        return value == null ? Constants.E_FAIL : WiceCommons.S_OK;
    }
}
