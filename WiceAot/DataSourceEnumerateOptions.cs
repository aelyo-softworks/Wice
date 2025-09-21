namespace Wice;

/// <summary>
/// Provides options that influence how a data source is enumerated and converted to text.
/// </summary>
public class DataSourceEnumerateOptions
{
    /// <summary>
    /// Optional member name to read from each item during enumeration.
    /// </summary>
    public virtual string? Member { get; set; }

    /// <summary>
    /// Optional format string applied to the selected value when converting to text.
    /// </summary>
    public virtual string? Format { get; set; }
}
