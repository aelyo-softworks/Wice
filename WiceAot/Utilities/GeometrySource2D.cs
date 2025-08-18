namespace Wice.Utilities;

/// <summary>
/// Represents a 2D geometry source that can interop with Windows.Graphics IGeometrySource2D.
/// Encapsulates a Direct2D <see cref="ID2D1Geometry"/> and a stable unique key used for identity/equality.
/// </summary>
/// <remarks>
/// Equality semantics:
/// - Two instances are considered equal if they reference the same <see cref="Geometry"/> instance,
///   or if their <see cref="UniqueKey"/> values are equal.
/// Hash code is derived from <see cref="UniqueKey"/> to remain stable regardless of geometry instance changes.
/// Explicit interop methods return HRESULT values (S_OK or E_FAIL) as expected by WinRT interop.
/// </remarks>
/// <seealso cref="IGeometrySource2D"/>
/// <seealso cref="Windows.Graphics.IGeometrySource2DInterop"/>
public partial class GeometrySource2D : IGeometrySource2D, Windows.Graphics.IGeometrySource2DInterop, IEquatable<GeometrySource2D>
{
    /// <summary>
    /// Initializes a new instance of <see cref="GeometrySource2D"/>.
    /// </summary>
    /// <param name="uniqueKey">A non-null, stable identifier used for equality and hashing.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uniqueKey"/> is null.</exception>
    public GeometrySource2D(string uniqueKey)
    {
        ArgumentNullException.ThrowIfNull(uniqueKey);
        UniqueKey = uniqueKey;
    }

    /// <summary>
    /// Gets the stable identity key for this geometry source.
    /// </summary>
    public string UniqueKey { get; }

    /// <summary>
    /// Gets or sets the underlying Direct2D geometry.
    /// May be null until populated; interop APIs will fail (E_FAIL) when null.
    /// </summary>
    public ID2D1Geometry? Geometry { get; set; }

    /// <inheritdoc/>
    public override int GetHashCode() => UniqueKey.GetHashCode();

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as GeometrySource2D);

    /// <summary>
    /// Determines whether the specified <see cref="GeometrySource2D"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The other instance to compare to.</param>
    /// <returns>
    /// True if both reference the same <see cref="Geometry"/> instance or their <see cref="UniqueKey"/> values match; otherwise false.
    /// </returns>
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
