namespace Wice;

/// <summary>
/// Defines selection behavior for controls that support item selection.
/// </summary>
/// <remarks>
/// Use <see cref="SelectionMode.Single"/> to restrict selection to a single item,
/// or <see cref="SelectionMode.Multiple"/> to allow selecting more than one item.
/// </remarks>
public enum SelectionMode
{
    /// <summary>
    /// Only one item can be selected at a time.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple items can be selected concurrently.
    /// </summary>
    Multiple,
}
