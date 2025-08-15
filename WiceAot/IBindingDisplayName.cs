namespace Wice;

/// <summary>
/// Defines a contract for producing a human-readable display name for a binding,
/// optionally based on a provided context.
/// </summary>
public interface IBindingDisplayName
{
    /// <summary>
    /// Gets a human-readable name that describes the binding for the specified context.
    /// </summary>
    /// <param name="context">
    /// An optional context object that may influence the resulting display name, such as
    /// a data item, target control, or binding source. Implementations should accept null.
    /// </param>
    /// <returns>
    /// A display-friendly name for the binding. Implementations should not return null;
    /// return an empty string if no name can be determined.
    /// </returns>
    string GetName(object context);
}
