namespace Wice;

/// <summary>
/// Defines a contract for objects that expose a single child <see cref="Visual"/> as their content.
/// </summary>
public interface IContentParent
{
    /// <summary>
    /// Gets the visual that represents this element's content.
    /// </summary>
    /// <value>
    /// The child <see cref="Visual"/> hosted by this parent.
    /// </value>
    Visual Content { get; }
}
