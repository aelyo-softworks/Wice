namespace Wice;

/// <summary>
/// Provides options that influence how a data source is enumerated and converted to text.
/// </summary>
/// <remarks>
/// - Member: If specified, the enumerator should read the given property or field from each item.
///   If null, the item itself is used.
/// - Format: If specified, the selected value is formatted using standard .NET formatting;
///   if null or empty, default formatting is used.
/// </remarks>
public class DataSourceEnumerateOptions
{
    /// <summary>
    /// Optional member name to read from each item during enumeration.
    /// </summary>
    /// <remarks>
    /// When set, the enumerator should extract the value of the given property or field from each item.
    /// If null, the raw item is used.
    /// </remarks>
    public virtual string? Member { get; set; }

    /// <summary>
    /// Optional format string applied to the selected value when converting to text.
    /// </summary>
    /// <remarks>
    /// Uses standard .NET formatting (e.g., "G", "N2", "yyyy-MM-dd"). If null or empty, default formatting is used.
    /// </remarks>
    public virtual string? Format { get; set; }
}
