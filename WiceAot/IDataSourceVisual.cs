namespace Wice;

/// <summary>
/// Represents a visual element that can bind itself to a data source and optionally
/// create and bind child visuals for the data items.
/// </summary>
public interface IDataSourceVisual
{
    /// <summary>
    /// Occurs after the data source has been bound and the visual has been updated accordingly.
    /// </summary>
    event EventHandler<EventArgs>? DataBound;

    /// <summary>
    /// Gets or sets the data source to bind to.
    /// </summary>
    /// <value>
    /// An object representing the source of data. This can be a single object, a collection,
    /// or any provider understood by the <see cref="DataBinder"/> implementation.
    /// </value>
    object? DataSource { get; set; }

    /// <summary>
    /// Gets or sets the name of the member on the data item to use during binding or display.
    /// </summary>
    /// <value>
    /// The member name (for example, a property or field name) that identifies the value to bind.
    /// This may be a simple name or an implementation-specific path.
    /// </value>
    string? DataItemMember { get; set; }

    /// <summary>
    /// Gets or sets the format string applied to the data item or member value when producing display text.
    /// </summary>
    /// <value>
    /// A composite format string compatible with <see cref="string.Format(string, object?)"/> semantics,
    /// such as "{0}", "{0:N2}", or "{0:yyyy-MM-dd}".
    /// </value>
    string? DataItemFormat { get; set; }

    /// <summary>
    /// Gets or sets the binder responsible for creating and binding visuals for the current data source.
    /// </summary>
    /// <value>
    /// A <see cref="DataBinder"/> that drives binding behavior, including visual creation and item binding.
    /// </value>
    DataBinder? DataBinder { get; set; }

    /// <summary>
    /// Binds the current <see cref="DataSource"/> using the configured <see cref="DataBinder"/> and options.
    /// </summary>
    void BindDataSource();
}
