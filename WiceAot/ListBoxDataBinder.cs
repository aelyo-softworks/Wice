namespace Wice;

/// <summary>
/// Specialized <see cref="DataBinder"/> for list-style controls that may render separators between items.
/// </summary>
public class ListBoxDataBinder : DataBinder
{
    /// <summary>
    /// Delegate that creates and/or configures the separator visual associated with a list item.
    /// </summary>
    public Action<ListBoxDataBindContext>? SeparatorVisualCreator { get; set; }
}
