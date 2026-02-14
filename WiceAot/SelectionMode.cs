namespace Wice;

/// <summary>
/// Defines selection behavior for visuals that support item selection.
/// </summary>
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
