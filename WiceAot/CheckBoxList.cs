namespace Wice;

/// <summary>
/// A <see cref="StateButtonListBox"/> that prepends a <see cref="CheckBox"/> to each item
/// and keeps the item's selection synchronized with the checkbox value.
/// </summary>
/// <remarks>
/// Behavior:
/// - <see cref="CreateStateButton(DataBindContext)"/>: creates a per-item <see cref="CheckBox"/> and toggles the item's selection on click.
/// - <see cref="UpdateItemSelection(ItemVisual, bool?)"/>: propagates selection changes to the embedded <see cref="CheckBox"/>.
/// </remarks>
/// <seealso cref="StateButtonListBox"/>
/// <seealso cref="CheckBox"/>
public partial class CheckBoxList : StateButtonListBox
{
    /// <summary>
    /// Creates the per-item <see cref="CheckBox"/> and wires its click to toggle the item's selection.
    /// </summary>
    /// <param name="context">The data-bind context for the current item.</param>
    /// <returns>A configured, non-null <see cref="CheckBox"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    protected override StateButton CreateStateButton(DataBindContext context)
    {
        ExceptionExtensions.ThrowIfNull(context, nameof(context));
        var cb = new CheckBox();
        cb.Click += (s, e) =>
        {
            if (context.ItemVisual != null)
            {
                context.ItemVisual.IsSelected = cb.Value;
            }
        };
        return cb;
    }

    /// <summary>
    /// Applies a selection state to an item visual and synchronizes the embedded <see cref="CheckBox"/> value.
    /// </summary>
    /// <param name="visual">The item visual whose selection state should be updated.</param>
    /// <param name="select">
    /// The new selection state:
    /// - true to select,
    /// - false to unselect,
    /// - null to only refresh brushes/visual state without changing selection.
    /// </param>
    /// <returns>True when the selection state changed; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visual"/> is null.</exception>
    public override bool UpdateItemSelection(ItemVisual visual, bool? select)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        var changed = base.UpdateItemSelection(visual, select);
        var cb = visual.AllChildren.OfType<CheckBox>().FirstOrDefault();
        if (cb != null)
        {
            cb.Value = visual.IsSelected;
        }
        return changed;
    }
}
