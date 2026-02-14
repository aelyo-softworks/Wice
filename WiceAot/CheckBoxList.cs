namespace Wice;

/// <summary>
/// A <see cref="StateButtonListBox"/> that prepends a <see cref="CheckBox"/> to each item
/// and keeps the item's selection synchronized with the checkbox value.
/// </summary>
public partial class CheckBoxList : StateButtonListBox
{
    /// <inheritdoc/>
    protected override StateButton CreateStateButton(DataBindContext context)
    {
        ExceptionExtensions.ThrowIfNull(context, nameof(context));
        var cb = new CheckBox();
        cb.Click += (s, e) =>
        {
            context.ItemVisual?.IsSelected = cb.Value;
        };
        return cb;
    }

    /// <inheritdoc/>
    public override bool UpdateItemSelection(ItemVisual visual, bool? select)
    {
        ExceptionExtensions.ThrowIfNull(visual, nameof(visual));
        var changed = base.UpdateItemSelection(visual, select);
        var cb = visual.AllChildren.OfType<CheckBox>().FirstOrDefault();
        cb?.Value = visual.IsSelected;
        return changed;
    }
}
