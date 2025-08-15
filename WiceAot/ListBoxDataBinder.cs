namespace Wice;

/// <summary>
/// Specialized <see cref="DataBinder"/> for list-style controls that may render separators between items.
/// </summary>
/// <remarks>
/// This binder participates in the standard data-binding pipeline exposed by <see cref="DataBinder"/>:
/// 1. <see cref="DataBinder.ItemVisualCreator"/> creates/configures the per-item container (<c>ItemVisual</c>).
/// 2. <see cref="DataBinder.DataItemVisualCreator"/> creates/configures the primary data visual for the item.
/// 3. <see cref="DataBinder.DataItemVisualBinder"/> binds the data item to the visuals.
/// In addition, <see cref="SeparatorVisualCreator"/> can be provided to insert an optional separator visual
/// associated with each item (typically after the data visual within the same item container).
/// Implementations commonly use <see cref="ListBoxDataBindContext.Index"/> and
/// <see cref="ListBoxDataBindContext.IsLast"/> to decide when to show or hide the separator.
/// </remarks>
/// <seealso cref="DataBinder"/>
/// <seealso cref="DataBindContext"/>
/// <seealso cref="ListBoxDataBindContext"/>
/// <seealso cref="Visual"/>
public class ListBoxDataBinder : DataBinder
{
    /// <summary>
    /// Delegate that creates and/or configures the separator visual associated with a list item.
    /// </summary>
    /// <remarks>
    /// The delegate receives a <see cref="ListBoxDataBindContext"/> that provides:
    /// - <see cref="DataBindContext.Data"/>: the source data item.
    /// - <see cref="DataBindContext.ItemVisual"/>: the per-item container to which a separator is typically added.
    /// - <see cref="ListBoxDataBindContext.Index"/>: zero-based item index.
    /// - <see cref="ListBoxDataBindContext.IsLast"/>: whether this is the last item (often used to skip separators).
    /// Implementations are expected to:
    /// - Create and attach a separator <see cref="Visual"/> to <see cref="DataBindContext.ItemVisual"/> as appropriate.
    /// - Assign the created separator to <see cref="ListBoxDataBindContext.SeparatorVisual"/> on the context.
    /// This delegate is optional; when null, no separators are created.
    /// </remarks>
    public Action<ListBoxDataBindContext>? SeparatorVisualCreator { get; set; }
}
