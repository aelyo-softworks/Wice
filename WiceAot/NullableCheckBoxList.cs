namespace Wice;

/// <summary>
/// A <see cref="StateButtonListBox"/> whose per-item state button is a tri-state <see cref="NullableCheckBox"/>.
/// </summary>
/// <remarks>
/// Behavior:
/// - Each item is prefixed with a <see cref="NullableCheckBox"/> created by <see cref="CreateStateButton(DataBindContext)"/>.
/// - Clicking the checkbox sets the item's <see cref="ItemVisual.IsSelected"/> to <c>true</c> only when
///   the checkbox <see cref="NullableCheckBox.Value"/> is exactly <c>true</c> (unchecked or indeterminate do not select).
/// - Selection synchronization is one-way: when an item becomes selected, the checkbox is forced to <c>true</c>;
///   when an item is unselected, the checkbox value is not changed.
/// </remarks>
/// <seealso cref="StateButtonListBox"/>
/// <seealso cref="NullableCheckBox"/>
public partial class NullableCheckBoxList : StateButtonListBox
{
    /// <summary>
    /// Creates the per-item <see cref="NullableCheckBox"/> and wires its click handler to update item selection.
    /// </summary>
    /// <param name="context">The data-bind context for the current item.</param>
    /// <returns>A configured <see cref="NullableCheckBox"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// On click, if <see cref="DataBindContext.ItemVisual"/> is not <see langword="null"/>, sets
    /// <see cref="ItemVisual.IsSelected"/> to <c>true</c> only when the checkbox value is <c>true</c>.
    /// </remarks>
    protected override StateButton CreateStateButton(DataBindContext context)
    {
        ExceptionExtensions.ThrowIfNull(context, nameof(context));
        var ncb = new NullableCheckBox();
        ncb.Click += (s, e) =>
        {
            if (context.ItemVisual != null)
            {
                context.ItemVisual.IsSelected = ncb.Value == true;
            }
        };
        return ncb;
    }

    /// <summary>
    /// Applies the selection state to an item and, when selected, synchronizes the first <see cref="NullableCheckBox"/> child to <c>true</c>.
    /// </summary>
    /// <param name="visual">The item visual whose selection state is being updated.</param>
    /// <param name="select">
    /// Selection directive:
    /// - <c>true</c> to select the item,
    /// - <c>false</c> to unselect the item,
    /// - <c>null</c> to refresh visuals without changing logical selection.
    /// </param>
    /// <returns><see langword="true"/> if the selection state changed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visual"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// After delegating to the base implementation, if the item is selected, sets the checkbox's
    /// <see cref="NullableCheckBox.Value"/> to <c>true</c>. When unselected, the checkbox value is left unchanged.
    /// </remarks>
    public override bool UpdateItemSelection(ItemVisual visual, bool? select)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        var changed = base.UpdateItemSelection(visual, select);
        var ncb = visual.AllChildren.OfType<NullableCheckBox>().FirstOrDefault();
        if (ncb != null)
        {
            if (visual.IsSelected)
            {
                ncb.Value = true;
            }
        }
        return changed;
    }
}
