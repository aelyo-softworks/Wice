namespace Wice;

/// <summary>
/// A <see cref="StateButtonListBox"/> whose per-item state button is a tri-state <see cref="NullableCheckBox"/>.
/// </summary>
public partial class NullableCheckBoxList : StateButtonListBox
{
    /// <summary>
    /// Creates the per-item <see cref="NullableCheckBox"/> and wires its click handler to update item selection.
    /// </summary>
    /// <param name="context">The data-bind context for the current item.</param>
    /// <returns>A configured <see cref="NullableCheckBox"/> instance.</returns>
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
