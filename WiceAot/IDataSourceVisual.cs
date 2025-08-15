namespace Wice;

/// <summary>
/// Represents a visual element that can bind itself to a data source and optionally
/// create and bind child visuals for the data items.
/// </summary>
/// <remarks>
/// Implementations are expected to coordinate data binding through <see cref="DataBinder"/> and
/// raise <see cref="DataBound"/> after a binding operation completes.
/// </remarks>
public interface IDataSourceVisual
{
    /// <summary>
    /// Occurs after the data source has been bound and the visual has been updated accordingly.
    /// </summary>
    /// <remarks>
    /// Implementations should raise this event upon successful completion of <see cref="BindDataSource"/>.
    /// </remarks>
    event EventHandler<EventArgs>? DataBound;

    /// <summary>
    /// Gets or sets the data source to bind to.
    /// </summary>
    /// <value>
    /// An object representing the source of data. This can be a single object, a collection,
    /// or any provider understood by the <see cref="DataBinder"/> implementation.
    /// </value>
    /// <remarks>
    /// Setting this property typically does not perform binding immediately; call <see cref="BindDataSource"/>
    /// to (re)apply the binding. Setting it to <see langword="null"/> clears the current binding.
    /// </remarks>
    /// <seealso cref="DataBinder"/>
    object? DataSource { get; set; }

    /// <summary>
    /// Gets or sets the name of the member on the data item to use during binding or display.
    /// </summary>
    /// <value>
    /// The member name (for example, a property or field name) that identifies the value to bind.
    /// This may be a simple name or an implementation-specific path.
    /// </value>
    /// <remarks>
    /// The semantics of this value are defined by the <see cref="DataBinder"/> and how it interprets members
    /// on the data item(s).
    /// </remarks>
    /// <seealso cref="DataBinder"/>
    string? DataItemMember { get; set; }

    /// <summary>
    /// Gets or sets the format string applied to the data item or member value when producing display text.
    /// </summary>
    /// <value>
    /// A composite format string compatible with <see cref="string.Format(string, object?)"/> semantics,
    /// such as "{0}", "{0:N2}", or "{0:yyyy-MM-dd}".
    /// </value>
    /// <remarks>
    /// The <see cref="DataBinder"/> may use this value to format text-based visuals. If <see langword="null"/>,
    /// no additional formatting is applied beyond the binder's defaults.
    /// </remarks>
    /// <seealso cref="DataBinder"/>
    string? DataItemFormat { get; set; }

    /// <summary>
    /// Gets or sets the binder responsible for creating and binding visuals for the current data source.
    /// </summary>
    /// <value>
    /// A <see cref="DataBinder"/> that drives binding behavior, including visual creation and item binding.
    /// </value>
    /// <remarks>
    /// The binder may use <see cref="DataBindContext"/> to pass information during binding.
    /// </remarks>
    /// <seealso cref="DataBinder"/>
    /// <seealso cref="DataBindContext"/>
    DataBinder? DataBinder { get; set; }

    /// <summary>
    /// Binds the current <see cref="DataSource"/> using the configured <see cref="DataBinder"/> and options.
    /// </summary>
    /// <remarks>
    /// Implementations should ensure the visual reflects the latest data and raise <see cref="DataBound"/>
    /// upon completion.
    /// </remarks>
    /// <seealso cref="DataSource"/>
    /// <seealso cref="DataBinder"/>
    void BindDataSource();
}
