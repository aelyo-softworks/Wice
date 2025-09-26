namespace Wice;

/// <summary>
/// Provides contextual information for data binding operations,
/// including the bound <see cref="Data"/> item, the visuals involved in the binding,
/// and a case-insensitive extensible property bag.
/// </summary>
/// <param name="data">The source data object being bound. May be <see langword="null"/>.</param>
public class DataBindContext(object? data)
{
    private readonly Dictionary<string, object> _properties = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the data item this context is wrapping.
    /// </summary>
    public object? Data { get; } = data;

    /// <summary>
    /// Gets or sets the container visual created for the item (e.g., a list item host).
    /// Can be assigned by <see cref="DataBinder.ItemVisualAdded"/>.
    /// </summary>
    public virtual ItemVisual? ItemVisual { get; set; }

    /// <summary>
    /// Gets or sets the primary visual representing the <see cref="Data"/> item.
    /// Typically created by <see cref="DataBinder.DataItemVisualCreator"/> and then bound by
    /// <see cref="DataBinder.DataItemVisualBinder"/>.
    /// </summary>
    public virtual Visual? DataVisual { get; set; }

    /// <summary>
    /// Gets an extensible, case-insensitive property bag for passing ad-hoc values
    /// across creation/binding phases. Keys are compared using <see cref="StringComparer.OrdinalIgnoreCase"/>.
    /// </summary>
    public virtual IDictionary<string, object> Properties => _properties;

    /// <summary>
    /// Gets a display-friendly name for the current <see cref="Data"/> item.
    /// </summary>
    /// <param name="context">
    /// Optional context forwarded to <see cref="IBindingDisplayName.GetName(object)"/> when <see cref="Data"/>
    /// implements <see cref="IBindingDisplayName"/>. When <see langword="null"/>, this instance is used.
    /// </param>
    /// <returns>
    /// - The string itself when <see cref="Data"/> is a <see cref="string"/>;<br/>
    /// - The result of <see cref="IBindingDisplayName.GetName(object)"/> when implemented and non-null;<br/>
    /// - Otherwise, <see cref="object.ToString"/> of <see cref="Data"/> (via <see cref="string.Format(string, object?)"/>).
    /// </returns>
    public virtual string GetDisplayName(object? context = null)
    {
        context ??= this;
        if (Data is string str)
            return str;

        if (Data is IBindingDisplayName name)
        {
            str = name.GetName(context);
            if (str != null)
                return str;
        }

        return string.Format("{0}", Data);
    }
}
