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

    /// <summary>
    /// Attempts to obtain a geometry using the provided factory.
    /// </summary>
    /// <param name="factory">The Direct2D factory that can be used to create or retrieve the geometry.</param>
    /// <param name="value">When this method returns, contains the geometry if successful.</param>
    /// <returns>HRESULT indicating success or failure.</returns>
    /// <remarks>
    /// Not implemented in this type; callers should prefer <see cref="Windows.Graphics.IGeometrySource2DInterop.GetGeometry(out ID2D1Geometry)"/>.
    /// </remarks>
    HRESULT Windows.Graphics.IGeometrySource2DInterop.TryGetGeometryUsingFactory(ID2D1Factory factory, out ID2D1Geometry value) => throw new NotImplementedException();

    /// <summary>
    /// Retrieves the current geometry instance for interop consumers.
    /// </summary>
    /// <param name="value">When this method returns, contains the current <see cref="ID2D1Geometry"/> if available.</param>
    /// <returns>
    /// S_OK when <see cref="Geometry"/> is not null and is returned in <paramref name="value"/>; otherwise E_FAIL.
    /// </returns>
    /// <remarks>
    /// This method does not create a new geometry; it only exposes the existing <see cref="Geometry"/> reference.
    /// </remarks>
    HRESULT Windows.Graphics.IGeometrySource2DInterop.GetGeometry(out ID2D1Geometry value)
    {
        value = Geometry!;
        return value == null ? Constants.E_FAIL : WiceCommons.S_OK;
    }
}
